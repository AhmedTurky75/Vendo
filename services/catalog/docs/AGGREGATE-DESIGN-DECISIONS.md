# Aggregate Design Decisions

## Table of Contents
- [Introduction](#introduction)
- [What is an Aggregate?](#what-is-an-aggregate)
- [Our Aggregates](#our-aggregates)
- [Design Decisions](#design-decisions)
- [Alternatives Considered](#alternatives-considered)
- [Trade-offs Made](#trade-offs-made)
- [Aggregate Rules](#aggregate-rules)
- [Common Pitfalls Avoided](#common-pitfalls-avoided)

---

## Introduction

This document explains **every aggregate boundary decision** made in the Catalog Service. Understanding these decisions will help you:
- Learn how to identify aggregate boundaries
- Understand the trade-offs between consistency and performance
- Avoid common aggregate design mistakes
- Apply these principles to your own domain models

**Key Principle:** Aggregates are consistency boundaries, not just object graphs.

---

## What is an Aggregate?

### Definition

An **Aggregate** is a cluster of domain objects that can be treated as a single unit for data changes.

### Key Characteristics

1. **Consistency Boundary**: All invariants within the aggregate must be protected
2. **Transaction Boundary**: Changes to an aggregate are saved in a single transaction
3. **Single Root**: Only the aggregate root can be referenced from outside
4. **Identity**: Each aggregate has a unique identity (the root's ID)
5. **Lifecycle**: The entire aggregate is loaded and saved together

### Why Aggregates Matter

```csharp
// ❌ WITHOUT AGGREGATES: Business rules can be broken
product.Price = -100m; // Nothing prevents this!
product.StockQuantity = -50; // Or this!
await repository.SaveAsync(product); // Invalid state persisted!

// ✅ WITH AGGREGATES: Business rules are enforced
var newPrice = Money.Create(-100m); // Returns null - validation failed
product.ChangePrice(newPrice); // Won't execute - newPrice is null
// Invalid state cannot exist!
```

---

## Our Aggregates

### Aggregate 1: Product Aggregate

**Aggregate Root:** `Product` entity

**Included in Boundary:**
- ✅ Product (root entity)
- ✅ SKU (value object)
- ✅ Money (value object for Price and CompareAtPrice)
- ✅ ProductImages (value object collection)
- ✅ SEOMetadata (value object)
- ✅ Dimensions (value object)
- ✅ Tags (collection of strings)

**NOT Included in Boundary:**
- ❌ Category (separate aggregate)
- ❌ Tenant (separate aggregate in different bounded context)
- ❌ Orders containing this product (separate aggregate in Order context)

**Why This Boundary?**

```csharp
public class Product : BaseEntity
{
    // THESE ARE PROTECTED TOGETHER (same aggregate)
    private SKU _sku;                    // Product identifier
    private Money _price;                // Current price
    private Money? _compareAtPrice;      // Original price for discounts
    private ProductImages _images;       // Product images

    // INVARIANT: CompareAtPrice must be > Price
    public void SetCompareAtPrice(Money compareAtPrice, string updatedBy)
    {
        if (!compareAtPrice.IsGreaterThan(_price))
            throw new InvalidOperationException(
                "Compare-at price must be greater than current price");

        _compareAtPrice = compareAtPrice;
        UpdateAudit(updatedBy);
    }
}
```

**The invariant** "CompareAtPrice > Price" involves two properties of the same product. They must be in the same aggregate to enforce this rule atomically.

---

### Aggregate 2: Category Aggregate

**Aggregate Root:** `Category` entity

**Included in Boundary:**
- ✅ Category (root entity)
- ✅ Slug (value object)
- ✅ SEOMetadata (value object)
- ✅ ParentCategoryId (reference to parent - ID only)

**NOT Included in Boundary:**
- ❌ Parent Category (separate aggregate instance)
- ❌ Child Categories (separate aggregate instances)
- ❌ Products in this category (separate aggregates)

**Why This Boundary?**

```csharp
public class Category : BaseEntity
{
    // THESE ARE PROTECTED TOGETHER
    private string _name;
    private Slug _slug;
    private SEOMetadata? _seoMetadata;

    // REFERENCE to parent (not included in aggregate)
    public Guid? ParentCategoryId { get; private set; }

    // INVARIANT: Category cannot be its own parent
    public void SetParent(Guid? parentId, string updatedBy)
    {
        if (parentId.HasValue && parentId.Value == Id)
            throw new InvalidOperationException(
                "Category cannot be its own parent");

        ParentCategoryId = parentId;
        UpdateAudit(updatedBy);
    }
}
```

**Important:** We store `ParentCategoryId` (reference) not `ParentCategory` (object). This keeps aggregates separate while maintaining relationships.

---

## Design Decisions

### Decision 1: Product and Category are Separate Aggregates

**Decision:** Product and Category are separate aggregates, connected by ID reference only.

**Reasoning:**

1. **Different Lifecycles**
   - Categories are created/managed independently by administrators
   - Products are created/updated frequently by merchants
   - Deleting a category doesn't delete products (they're reassigned)

2. **Different Transaction Boundaries**
   ```csharp
   // Scenario: Changing category name
   // Should this lock ALL products in that category? NO!

   // ✅ CORRECT: Only lock the category
   var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
   category.ChangeName("New Name", "admin");
   await _unitOfWork.SaveChangesAsync(); // Only locks category row

   // ❌ WRONG: If Product contained Category entity
   // Updating category would require loading ALL products
   // This could be thousands of products!
   ```

3. **Scalability**
   - A category can have thousands of products
   - Loading all products just to change category name = performance disaster
   - Separate aggregates allow independent scaling

**Implementation:**

```csharp
public class Product : BaseEntity
{
    // ✅ Reference by ID (weak reference)
    public Guid CategoryId { get; private set; }

    // ❌ NOT this (strong reference)
    // public Category Category { get; private set; }

    public void ChangeCategory(Guid newCategoryId, string updatedBy)
    {
        // We don't need to load the Category object
        // We just verify the ID exists (application layer responsibility)
        CategoryId = newCategoryId;
        UpdateAudit(updatedBy);
    }
}
```

---

### Decision 2: ProductImages as Value Object Collection

**Decision:** ProductImages is a value object contained within Product aggregate, not a separate entity.

**Reasoning:**

1. **No Independent Identity**
   - An image has no meaning without its product
   - Images are always shown in context of a product

2. **Managed as a Unit**
   ```csharp
   // Images are always managed together with product
   var images = ProductImages.Create(new List<string> {
       "https://cdn.example.com/product1.jpg",
       "https://cdn.example.com/product2.jpg"
   });

   var product = Product.Create(
       tenantId, categoryId, name, description,
       sku, price, images, "admin"
   );
   ```

3. **Consistency Requirements**
   - INVARIANT: "At least one image when product is published"
   - This rule involves both Product.Status and ProductImages
   - Must be in same aggregate to enforce atomically

**Implementation:**

```csharp
public class Product : BaseEntity
{
    private ProductImages? _images;

    // INVARIANT: Published products must have images
    public void Publish(string updatedBy)
    {
        if (_images == null || !_images.Urls.Any())
            throw new InvalidOperationException(
                "Cannot publish product without images");

        Status = ProductStatus.Active;
        UpdateAudit(updatedBy);
        AddDomainEvent(new ProductPublishedEvent(Id, Name));
    }
}
```

---

### Decision 3: Price and CompareAtPrice as Separate Money Instances

**Decision:** Store Price and CompareAtPrice as separate Money value objects, not as a single PriceRange value object.

**Reasoning:**

1. **Different Optionality**
   - Price is required (every product has a price)
   - CompareAtPrice is optional (only for discounted products)

2. **Different Business Operations**
   ```csharp
   // ✅ CORRECT: Independent operations
   product.ChangePrice(newPrice, "admin");        // Changes current price
   product.SetCompareAtPrice(compareAt, "admin"); // Sets discount reference
   product.ClearCompareAtPrice("admin");          // Removes discount

   // ❌ WRONG: If combined in PriceRange
   // Would need complex "change one but not the other" logic
   product.UpdatePriceRange(new PriceRange(price, compareAt));
   ```

3. **Domain Events**
   - Price changes raise `ProductPriceChangedEvent` (critical for integrations)
   - CompareAtPrice changes are less significant
   - Separate properties = separate events

**Alternative Considered:** `PriceRange` value object

```csharp
// ❌ REJECTED ALTERNATIVE
public class PriceRange : ValueObject
{
    public Money CurrentPrice { get; }
    public Money? OriginalPrice { get; }

    public decimal DiscountPercentage =>
        OriginalPrice != null
            ? ((OriginalPrice.Amount - CurrentPrice.Amount) / OriginalPrice.Amount) * 100
            : 0;
}

// Why rejected?
// 1. Violates single responsibility (handles both pricing and discounts)
// 2. Makes "change price only" awkward
// 3. Harder to version/evolve independently
```

---

### Decision 4: Category Hierarchy via References

**Decision:** Categories reference parent by ID, don't contain parent/children entities.

**Reasoning:**

1. **Prevent Infinite Recursion**
   ```csharp
   // ❌ DISASTER: Infinite loading
   public class Category
   {
       public Category ParentCategory { get; set; }
       public List<Category> ChildCategories { get; set; }
   }

   // Loading one category would load:
   // → Its parent
   //   → Parent's parent
   //     → Parent's parent's parent...
   // → Its children
   //   → Children's children
   //     → Children's children's children...
   // = Loads entire category tree!
   ```

2. **Independent Lifecycle**
   - Moving a category doesn't affect its children's data
   - Deleting parent can set children's ParentCategoryId to null
   - Each category change is isolated

3. **Query Flexibility**
   ```csharp
   // ✅ Can query hierarchy efficiently
   public async Task<List<Category>> GetCategoryPathAsync(Guid categoryId)
   {
       var path = new List<Category>();
       var current = await _repository.GetByIdAsync(categoryId);

       while (current != null)
       {
           path.Insert(0, current);
           current = current.ParentCategoryId.HasValue
               ? await _repository.GetByIdAsync(current.ParentCategoryId.Value)
               : null;
       }

       return path;
   }
   ```

**Implementation:**

```csharp
public class Category : BaseEntity
{
    // ✅ ID reference only
    public Guid? ParentCategoryId { get; private set; }

    // ❌ NOT entity reference
    // public Category? ParentCategory { get; private set; }
    // public List<Category> ChildCategories { get; private set; }

    public bool CanBeDeleted()
    {
        // Application layer checks if any children exist via query:
        // SELECT COUNT(*) FROM Categories WHERE ParentCategoryId = @id
        return true; // Domain doesn't know about children
    }
}
```

---

### Decision 5: SKU as Value Object Inside Product

**Decision:** SKU is a value object, not a separate aggregate or entity.

**Reasoning:**

1. **No Independent Existence**
   - SKU has no meaning without a product
   - You don't "browse SKUs" - you browse products

2. **Uniqueness is Cross-Aggregate**
   - SKU must be unique across all products (tenant-scoped)
   - This is enforced at infrastructure level (database unique index)
   - Not an aggregate concern

3. **Immutability Benefits**
   ```csharp
   // SKU is immutable - to change it, create new instance
   var oldSku = product.SKU; // SKU-001
   var newSku = SKU.Create("SKU-002");
   product.ChangeSKU(newSku, "admin"); // Replaces entire object

   // Can't do: product.SKU.Value = "SKU-002" (no setter!)
   ```

**Implementation:**

```csharp
public class Product : BaseEntity
{
    private SKU _sku;

    // SKU is exposed as value object (immutable)
    public SKU SKU => _sku;

    public void ChangeSKU(SKU newSku, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(newSku);

        // Application layer must verify uniqueness before calling this
        _sku = newSku;
        UpdateAudit(updatedBy);
    }
}
```

---

## Alternatives Considered

### Alternative 1: Product Contains Category Entity

**Considered Approach:**
```csharp
public class Product : BaseEntity
{
    public Category Category { get; private set; }

    public void ChangeCategory(Category newCategory, string updatedBy)
    {
        Category = newCategory;
        UpdateAudit(updatedBy);
    }
}
```

**Why Rejected:**

1. **Performance**: Loading a product would always load its category (N+1 queries)
2. **Consistency**: Two products in same category would each have separate Category copies
3. **Updates**: Updating category name would require updating all products in that category
4. **Aggregate Size**: Product aggregate would include Category aggregate (violates aggregate independence)

**Verdict:** ❌ Creates coupling, hurts performance, violates aggregate principles

---

### Alternative 2: Single Catalog Aggregate

**Considered Approach:**
```csharp
public class Catalog : BaseEntity  // Aggregate Root
{
    private List<Category> _categories;
    private List<Product> _products;

    public void AddProduct(Product product) { ... }
    public void AddCategory(Category category) { ... }
}
```

**Why Rejected:**

1. **Giant Aggregate**: All categories and products in one transaction boundary
2. **Concurrency**: Every product/category change would lock entire catalog
3. **Performance**: Loading catalog would load ALL products and categories
4. **Scalability**: Cannot distribute across multiple database shards

**Verdict:** ❌ Classic "aggregate too large" anti-pattern

---

### Alternative 3: ProductImage as Separate Aggregate

**Considered Approach:**
```csharp
public class ProductImage : BaseEntity  // Separate aggregate
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; }
    public int DisplayOrder { get; private set; }
}

// Separate repository
public interface IProductImageRepository
{
    Task<List<ProductImage>> GetByProductIdAsync(Guid productId);
    Task AddAsync(ProductImage image);
}
```

**Why Rejected:**

1. **Consistency Issues**:
   ```csharp
   // ❌ PROBLEM: Two transactions needed
   await _productRepository.AddAsync(product);
   await _productRepository.SaveChangesAsync(); // Transaction 1

   await _imageRepository.AddAsync(image);
   await _imageRepository.SaveChangesAsync();   // Transaction 2

   // What if second transaction fails?
   // Result: Product without images (broken invariant!)
   ```

2. **Complexity**: Simple image management becomes multi-aggregate coordination
3. **No Independent Behavior**: Images don't have business logic - they're just URLs
4. **Eventual Consistency Not Needed**: Images must be consistent with product immediately

**Verdict:** ❌ Over-engineering, adds complexity without benefits

---

### Alternative 4: Product Variants as Separate Aggregate

**Considered Approach:**
```csharp
public class ProductVariant : BaseEntity  // Separate from Product
{
    public Guid ProductId { get; private set; }
    public string Size { get; private set; }
    public string Color { get; private set; }
    public SKU SKU { get; private set; }
    public Money Price { get; private set; }
}
```

**Why We DON'T Have This (Yet):**

**Current Approach:** Our Product entity doesn't have variants. Each product is standalone.

**If We Add Variants Later:** We would need to decide:

**Option A: Variants Inside Product Aggregate (Recommended)**
```csharp
public class Product : BaseEntity
{
    private List<ProductVariant> _variants; // Value objects or child entities

    // INVARIANT: All variants must have unique SKU combinations
    public void AddVariant(ProductVariant variant, string updatedBy)
    {
        if (_variants.Any(v => v.SKU == variant.SKU))
            throw new InvalidOperationException("Duplicate SKU");

        _variants.Add(variant);
    }
}
```
✅ Good when: < 50 variants per product, variants share business rules

**Option B: Variants as Separate Aggregates**
```csharp
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
}
```
✅ Good when: > 50 variants per product, variants have independent lifecycle

**Current Decision:** Don't implement until needed (YAGNI principle)

---

## Trade-offs Made

### Trade-off 1: Consistency vs Performance

**Scenario:** Changing a category name

**Strong Consistency Approach (Rejected):**
```csharp
// ❌ Update all products when category changes
public async Task UpdateCategoryName(Guid categoryId, string newName)
{
    var category = await _categoryRepo.GetByIdAsync(categoryId);
    var products = await _productRepo.GetByCategoryAsync(categoryId);

    category.ChangeName(newName);

    // Update all products (denormalized category name)
    foreach (var product in products)
    {
        product.UpdateCategoryName(newName); // Denormalized!
    }

    await _unitOfWork.SaveChangesAsync(); // Locks category + all products
}
```
**Problems:** Slow, locks many rows, couples aggregates

**Eventual Consistency Approach (Chosen):**
```csharp
// ✅ Update category only, products reference by ID
public async Task UpdateCategoryName(Guid categoryId, string newName)
{
    var category = await _categoryRepo.GetByIdAsync(categoryId);
    category.ChangeName(newName, "admin");
    await _unitOfWork.SaveChangesAsync(); // Locks only category row

    // Products automatically get new name on next query (via join)
    // No denormalization, no product updates needed
}
```
**Benefits:** Fast, minimal locking, aggregates independent

**Trade-off:** We accept that query complexity increases (need joins) for better write performance and consistency boundaries.

---

### Trade-off 2: Aggregate Size vs Query Performance

**Scenario:** Displaying product with category breadcrumb

**Large Aggregate Approach (Rejected):**
```csharp
// Product includes full category object
var product = await _productRepo.GetByIdAsync(productId);
Console.WriteLine($"{product.Category.Name} > {product.Name}");
// ✅ No additional query needed
// ❌ Product aggregate now depends on Category aggregate
```

**Small Aggregate Approach (Chosen):**
```csharp
// Product references category by ID
var product = await _productRepo.GetByIdAsync(productId);
var category = await _categoryRepo.GetByIdAsync(product.CategoryId);
Console.WriteLine($"{category.Name} > {product.Name}");
// ❌ Two queries needed
// ✅ Aggregates remain independent
```

**Mitigation:** Use query models (read side) to optimize reads:
```csharp
// Query model for product display (CQRS read model)
public class ProductDisplayModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CategoryName { get; set; }  // Denormalized for reads
    public decimal Price { get; set; }
}

// Query handler joins in database (single query)
var result = await _context.Products
    .Join(_context.Categories, p => p.CategoryId, c => c.Id, (p, c) => new ProductDisplayModel {
        Id = p.Id,
        Name = p.Name,
        CategoryName = c.Name,
        Price = p.Price
    })
    .FirstOrDefaultAsync(p => p.Id == productId);
```

**Trade-off:** We accept query complexity (or CQRS read models) for better aggregate boundaries.

---

### Trade-off 3: Transactional Consistency vs Scalability

**Scenario:** Product stock management

**Single Aggregate Approach (Current):**
```csharp
public class Product : BaseEntity
{
    public int StockQuantity { get; private set; }

    public void UpdateStock(int quantity, string updatedBy)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative");

        StockQuantity = quantity;
        UpdateAudit(updatedBy);
        AddDomainEvent(new ProductStockChangedEvent(Id, StockQuantity));
    }
}
```
✅ Strong consistency - stock always accurate
❌ All stock changes lock product row (concurrency bottleneck)

**Separate Stock Aggregate Approach (Alternative for Future):**
```csharp
public class Stock : BaseEntity
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    public void Reserve(int quantity) { ... }
    public void Release(int quantity) { ... }
    public void Commit(int quantity) { ... }
}
```
✅ Better concurrency - stock locks separate from product locks
✅ Can scale stock management independently
❌ Eventual consistency - product and stock may be briefly out of sync

**Current Decision:** Keep stock in Product aggregate because:
1. Our current scale doesn't require separate stock aggregate
2. Strong consistency is more important than extreme scalability (for now)
3. Can refactor to separate aggregate later if needed (evolution)

**Future Trigger:** If we see >100 concurrent stock updates/second, consider separate aggregate

---

### Trade-off 4: Value Object Complexity vs Flexibility

**Scenario:** Product dimensions

**Simple Approach (Rejected):**
```csharp
public class Product
{
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string DimensionUnit { get; set; } // "cm", "in", etc.
}
```
✅ Simple, flexible
❌ No validation, can be inconsistent (L=10, W=20, Unit="invalid")

**Value Object Approach (Chosen):**
```csharp
public class Dimensions : ValueObject
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public string Unit { get; }

    public static Dimensions? Create(decimal length, decimal width, decimal height, string unit)
    {
        if (length <= 0 || width <= 0 || height <= 0) return null;
        if (!IsValidUnit(unit)) return null;

        return new Dimensions(length, width, height, unit.ToUpperInvariant());
    }
}
```
✅ Validation enforced, consistency guaranteed
❌ More code, harder to change dimensions (must create new instance)

**Trade-off:** We choose validation and consistency over simplicity. Invalid dimensions can cause shipping calculation errors, so protection is worth the complexity.

---

## Aggregate Rules

### Rule 1: One Aggregate Per Transaction

**Rule:** Each transaction should modify only ONE aggregate instance.

**Why:** Multiple aggregates = distributed transaction = complexity + failure risk

```csharp
// ✅ CORRECT: Single aggregate
public async Task<Result> UpdateProductPrice(Guid productId, decimal newPrice)
{
    var product = await _unitOfWork.Products.GetByIdAsync(productId);
    var money = Money.Create(newPrice);

    product.ChangePrice(money, "admin");

    await _unitOfWork.SaveChangesAsync(); // Single aggregate, single transaction
    return Result.Success();
}

// ❌ WRONG: Multiple aggregates
public async Task UpdateProductAndCategory(Guid productId, Guid categoryId)
{
    var product = await _unitOfWork.Products.GetByIdAsync(productId);
    var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

    product.ChangePrice(Money.Create(100), "admin");
    category.ChangeName("New Name", "admin");

    await _unitOfWork.SaveChangesAsync(); // Two aggregates in one transaction
    // What if category save fails but product succeeds? Inconsistency!
}
```

**Exception:** If you MUST update multiple aggregates, use domain events:

```csharp
// ✅ CORRECT: Update aggregates separately via events
public async Task UpdateProduct(Guid productId)
{
    var product = await _unitOfWork.Products.GetByIdAsync(productId);
    product.ChangePrice(Money.Create(100), "admin");

    // This raises ProductPriceChangedEvent
    await _unitOfWork.SaveChangesAsync();
}

// Event handler updates other aggregate
public class ProductPriceChangedHandler : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent notification)
    {
        // Update analytics aggregate in separate transaction
        var analytics = await _analyticsRepo.GetByProductIdAsync(notification.ProductId);
        analytics.RecordPriceChange(notification.NewPrice);
        await _analyticsRepo.SaveChangesAsync();
    }
}
```

---

### Rule 2: Reference Other Aggregates by ID Only

**Rule:** Aggregates reference other aggregates by identity (Guid), not by object reference.

```csharp
// ✅ CORRECT: ID reference
public class Product : BaseEntity
{
    public Guid CategoryId { get; private set; }
}

// ❌ WRONG: Object reference
public class Product : BaseEntity
{
    public Category Category { get; private set; }
}
```

**Why:**
- Prevents loading entire object graphs
- Maintains aggregate independence
- Allows each aggregate to be loaded/saved separately

---

### Rule 3: Enforce Invariants Within Aggregate

**Rule:** All business rules (invariants) that must be consistent should be in the same aggregate.

```csharp
public class Product : BaseEntity
{
    private Money _price;
    private Money? _compareAtPrice;

    // ✅ INVARIANT ENFORCED: CompareAtPrice > Price
    public void SetCompareAtPrice(Money compareAtPrice, string updatedBy)
    {
        // Both properties in same aggregate = can enforce atomically
        if (!compareAtPrice.IsGreaterThan(_price))
            throw new InvalidOperationException(
                "Compare-at price must be greater than current price");

        _compareAtPrice = compareAtPrice;
        UpdateAudit(updatedBy);
    }
}
```

**Counter-example (wrong):**
```csharp
// ❌ If Price and CompareAtPrice were in different aggregates
public class Product : BaseEntity
{
    public Guid PriceId { get; set; }           // Reference to Price aggregate
    public Guid? CompareAtPriceId { get; set; } // Reference to CompareAtPrice aggregate
}

// Can't enforce "CompareAtPrice > Price" because:
// 1. They're in different transactions
// 2. One could be updated without the other
// 3. Race conditions possible
```

---

### Rule 4: Keep Aggregates Small

**Rule:** Prefer smaller aggregates with eventual consistency over large aggregates with transactional consistency.

**Guideline:** If your aggregate has >5 entities or >1000 lines of code, it's probably too large.

**Our Aggregates:**
- Product: ~400 lines ✅ (manageable)
- Category: ~180 lines ✅ (small)

**Warning Signs of Too-Large Aggregates:**
- Loading the aggregate is slow (>100ms)
- Many unrelated business methods
- Frequent concurrent modification conflicts
- Entities within aggregate rarely change together

---

## Common Pitfalls Avoided

### Pitfall 1: Aggregate = Database Table

**Wrong Assumption:** "Each database table should be an aggregate."

**Reality:** Aggregates are about consistency boundaries, not data storage.

**Example:**
```csharp
// We have these tables:
// - Products (main table)
// - ProductImages (related table)

// ❌ WRONG: ProductImage as separate aggregate
public class ProductImage : BaseEntity { }

// ✅ CORRECT: ProductImages as value object within Product aggregate
public class Product : BaseEntity
{
    private ProductImages _images; // Value object
}

// In database, ProductImages can still be separate table (normalized)
// But in domain model, it's part of Product aggregate (consistency boundary)
```

**Lesson:** Aggregate boundaries ≠ table boundaries

---

### Pitfall 2: Navigating Between Aggregates

**Wrong Pattern:**
```csharp
// ❌ WRONG: Loading category through product navigation
var product = await _productRepo.GetByIdAsync(productId);
var categoryName = product.Category.Name; // Lazy loading or eager loading
```

**Right Pattern:**
```csharp
// ✅ CORRECT: Load aggregates separately
var product = await _productRepo.GetByIdAsync(productId);
var category = await _categoryRepo.GetByIdAsync(product.CategoryId);
var categoryName = category.Name;

// OR use query model (CQRS)
var productDisplay = await _queries.GetProductDisplayAsync(productId);
var categoryName = productDisplay.CategoryName; // Denormalized in read model
```

**Lesson:** Aggregates are loaded through repositories, not through navigation properties.

---

### Pitfall 3: Updating Multiple Aggregates in Business Logic

**Wrong Pattern:**
```csharp
// ❌ WRONG: Domain method updates multiple aggregates
public class Product : BaseEntity
{
    public void Discontinue(Category category, string updatedBy)
    {
        Status = ProductStatus.Discontinued;
        category.RemoveProduct(Id); // Modifying different aggregate!
        UpdateAudit(updatedBy);
    }
}
```

**Right Pattern:**
```csharp
// ✅ CORRECT: Raise event, let handler update other aggregate
public class Product : BaseEntity
{
    public void Discontinue(string updatedBy)
    {
        Status = ProductStatus.Discontinued;
        UpdateAudit(updatedBy);
        AddDomainEvent(new ProductDiscontinuedEvent(Id, CategoryId));
    }
}

// Separate handler updates category
public class ProductDiscontinuedHandler : INotificationHandler<ProductDiscontinuedEvent>
{
    public async Task Handle(ProductDiscontinuedEvent evt)
    {
        var category = await _categoryRepo.GetByIdAsync(evt.CategoryId);
        category.DecrementProductCount(); // Separate transaction
        await _categoryRepo.SaveChangesAsync();
    }
}
```

**Lesson:** Use domain events for cross-aggregate coordination.

---

### Pitfall 4: Anemic Aggregates

**Wrong Pattern:**
```csharp
// ❌ ANEMIC: No business logic, just getters/setters
public class Product
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

// Business logic in service layer
public class ProductService
{
    public async Task UpdateStock(Guid productId, int newQuantity)
    {
        var product = await _repo.GetByIdAsync(productId);
        product.StockQuantity = newQuantity; // No validation!
        await _repo.SaveAsync(product);
    }
}
```

**Right Pattern:**
```csharp
// ✅ RICH: Business logic encapsulated
public class Product : BaseEntity
{
    public int StockQuantity { get; private set; }

    public void UpdateStock(int quantity, string updatedBy)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative");

        StockQuantity = quantity;
        UpdateAudit(updatedBy);
        AddDomainEvent(new ProductStockChangedEvent(Id, StockQuantity));
    }
}

// Command handler just orchestrates
public class UpdateStockHandler : IRequestHandler<UpdateStockCommand>
{
    public async Task Handle(UpdateStockCommand request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        product.UpdateStock(request.Quantity, request.UpdatedBy); // Domain method!
        await _unitOfWork.SaveChangesAsync();
    }
}
```

**Lesson:** Put business logic in aggregates, not in services.

---

## Summary

### Key Takeaways

1. **Product and Category are separate aggregates** for scalability and independent lifecycle
2. **Value objects (SKU, Money, ProductImages) are inside aggregates** for consistency
3. **Aggregates reference each other by ID** to maintain independence
4. **Domain events coordinate cross-aggregate operations** for eventual consistency
5. **Aggregates are kept small** (< 5 entities each) for performance

### Decision Framework

When designing aggregates, ask:

1. **Must these objects be consistent together?** → Same aggregate
2. **Can they have independent lifecycles?** → Separate aggregates
3. **Do they change together frequently?** → Same aggregate
4. **Would loading them together be slow?** → Separate aggregates
5. **Is the relationship one-to-many with large "many"?** → Separate aggregates

### Further Learning

- Read `/docs/DDD-PATTERNS-CATALOG.md` for pattern implementations
- Read `/docs/UBIQUITOUS-LANGUAGE.md` for domain terminology
- Read `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` for what NOT to do
- Study `Product.cs` and `Category.cs` for full implementations

---

*This document is part of the Catalog Service educational documentation. Last updated: 2025-11-01*
