# DDD Patterns Catalog
## Complete Reference of Domain-Driven Design Patterns in the Catalog Service

This document catalogs every DDD pattern implemented in the Catalog Service, explaining where it's used, why it was chosen, and the problems it solves.

---

## Table of Contents
1. [Tactical Patterns](#tactical-patterns)
   - [Entities](#1-entities)
   - [Value Objects](#2-value-objects)
   - [Aggregates](#3-aggregates)
   - [Domain Events](#4-domain-events)
   - [Repositories](#5-repositories)
   - [Factory Methods](#6-factory-methods)
   - [Domain Services](#7-domain-services)
2. [Architectural Patterns](#architectural-patterns)
   - [Layered Architecture](#layered-architecture)
   - [CQRS](#cqrs)
   - [Unit of Work](#unit-of-work)
3. [Code Examples](#code-examples)

---

## Tactical Patterns

### 1. Entities

**Pattern Definition**: Objects that have a distinct identity that runs through time and different states.

#### 1.1 Product Entity

**Location**: `/src/Domain/Entities/Product.cs`

**Why It's an Entity**:
- Products have **unique identity** (ProductId) that persists throughout their lifecycle
- Two products with the same name and price are still different products
- Product state changes over time (draft → published, price changes, stock updates)
- **Identity matters more than attributes**

**Business Invariants Protected**:
1. **Price Consistency**: Compare-at-price must be greater than current price
2. **Stock Validity**: Stock quantity cannot be negative
3. **Publishing Rules**: Product must have name, valid price, and category to be published
4. **Cost vs. Selling Price**: Cost price should be less than selling price
5. **Name Validation**: Product name cannot be empty and max 200 characters

**Code Example**:
```csharp
// ✅ CORRECT: Two products with same attributes but different identity
var product1 = Product.Create(tenantId, categoryId, "Laptop", desc, sku1, price, "admin");
var product2 = Product.Create(tenantId, categoryId, "Laptop", desc, sku2, price, "admin");

// product1.Id != product2.Id → They are DIFFERENT products

// ❌ WRONG: Comparing products by attributes
if (product1.Name == product2.Name) {
    // This doesn't mean they're the same product!
}

// ✅ CORRECT: Comparing by identity
if (product1.Id == product2.Id) {
    // Now we're talking about the SAME product
}
```

**When to Use This Pattern**:
- ✅ Object has a unique identifier
- ✅ Object changes over time but remains "the same thing"
- ✅ Object has a lifecycle (created, modified, deleted)
- ✅ Equality is determined by ID, not attributes

**When NOT to Use**:
- ❌ Objects are interchangeable (use Value Object instead)
- ❌ No lifecycle or state changes (use Value Object instead)
- ❌ Equality is based on attributes (use Value Object instead)

---

#### 1.2 Category Entity

**Location**: `/src/Domain/Entities/Category.cs`

**Why It's an Entity**:
- Categories have **unique identity** independent of their name
- Renaming "Electronics" to "Tech" doesn't create a new category
- Categories have lifecycle and relationships (parent/child)
- **Identity matters**: Category with ID=1 is always the same category

**Business Invariants Protected**:
1. **Self-Reference Prevention**: Category cannot be its own parent
2. **Name Validation**: Category name cannot be empty, max 100 characters
3. **Deletion Rules**: Category can only be deleted if it has no products or child categories
4. **Activation State**: Categories can be activated/deactivated independently

**Code Example**:
```csharp
// ✅ CORRECT: Category identity persists through name changes
var category = Category.Create(tenantId, "Computers", desc, null, "admin");
var originalId = category.Id;

category.UpdateInformation("Tech Devices", desc, "admin");
// category.Id == originalId → Still the SAME category

// Business Rule Enforcement:
category.ChangeParent(category.Id, "admin");
// ❌ Throws: "Category cannot be its own parent"
```

**Key Design Decision**:
We made Category a separate aggregate root from Product because:
1. Categories have independent lifecycle
2. Categories can exist without products
3. Multiple products reference the same category
4. Category operations don't need product consistency

---

### 2. Value Objects

**Pattern Definition**: Objects that describe characteristics of things but have no conceptual identity. Defined by their attributes.

#### 2.1 Money Value Object

**Location**: `/src/Domain/ValueObjects/Money.cs`

**Why It's a Value Object**:
- $100 USD is always the same as another $100 USD
- **No identity**: We care about the amount and currency, not "which" $100
- **Immutable**: Money doesn't change; you create new money instances
- **Value equality**: Two Money objects with same amount/currency are identical

**Validation Rules**:
- Amount must be non-negative (≥ 0)
- Currency must be specified and valid (ISO 4217 codes)
- Operations preserve currency (can't add USD + EUR)

**Business Operations**:
```csharp
// ✅ CORRECT: Money operations create NEW instances
var price1 = Money.Create(100m, "USD")!;
var price2 = Money.Create(50m, "USD")!;
var total = price1.Add(price2); // Creates NEW Money(150, "USD")

// price1 is still Money(100, "USD") - IMMUTABILITY

// ✅ CORRECT: Value equality
var money1 = Money.Create(100m, "USD")!;
var money2 = Money.Create(100m, "USD")!;
// money1 == money2 → TRUE (same value, no identity)

// ❌ WRONG: Currency mismatch
var usd = Money.Create(100m, "USD")!;
var eur = Money.Create(100m, "EUR")!;
var sum = usd.Add(eur); // Throws InvalidOperationException!
```

**Why This Matters**:
```csharp
// 🚫 BEFORE (Primitive Obsession):
public class Product {
    public decimal Price { get; set; }
    // Which currency? No validation. Can be negative!
}
product.Price = -50m; // ❌ Compiles but makes no business sense

// ✅ AFTER (Value Object):
public class Product {
    private Money _price;
    public Money Price => _price;
}
product.ChangePrice(Money.Create(-50m));
// ❌ Returns null - validation prevents invalid state!
```

**Key Learning**: Value Objects **make invalid states unrepresentable**.

---

#### 2.2 SKU Value Object

**Location**: `/src/Domain/ValueObjects/SKU.cs`

**Why It's a Value Object**:
- SKU "PROD-001" is the same wherever it appears
- Format and validation rules matter, not identity
- Immutable - SKUs don't change, you assign a new one
- Perfect for ensuring format consistency

**Validation Rules**:
- Must be 3-50 characters
- Only alphanumeric, hyphens, underscores allowed
- Auto-normalized to UPPERCASE
- Regex pattern: `^[A-Z0-9\-_]{3,50}$`

**Real-World Example**:
```csharp
// ✅ CORRECT: Format enforcement
var sku1 = SKU.Create("prod-001");  // Returns SKU with "PROD-001"
var sku2 = SKU.Create("PROD-001");  // Returns SKU with "PROD-001"
// sku1 == sku2 → TRUE (normalized to same value)

// ❌ WRONG: Invalid formats rejected
var invalid1 = SKU.Create("AB");              // Too short → null
var invalid2 = SKU.Create("SKU WITH SPACES"); // Spaces → null
var invalid3 = SKU.Create("SKU@123");         // Special char → null

// ✅ CORRECT: Generation with prefix
var generated = SKU.Generate("LAPTOP");
// Returns: "LAPTOP-1A2B3C4D-5E6F" (guaranteed valid)
```

**Anti-Pattern Avoided**:
```csharp
// 🚫 BEFORE (String Primitive):
public class Product {
    public string SKU { get; set; }
}
product.SKU = "invalid sku!"; // ❌ Compiles, breaks at runtime

// ✅ AFTER (Value Object):
public class Product {
    private SKU _sku;
}
var sku = SKU.Create("invalid sku!"); // Returns null immediately
if (sku == null) {
    return Result.Failure("Invalid SKU format");
}
```

---

#### 2.3 Slug Value Object

**Location**: `/src/Domain/ValueObjects/Slug.cs`

**Why It's a Value Object**:
- URL-friendly identifier derived from product/category names
- Format rules are paramount (lowercase, hyphens, no special chars)
- No identity - "gaming-laptop" is the same everywhere
- Immutable - generate new one if name changes

**Transformation Rules**:
```csharp
// ✅ Slug Generation Examples:
Slug.Generate("Gaming Laptop")        → "gaming-laptop"
Slug.Generate("Café Menu")            → "cafe-menu"  (removes accents)
Slug.Generate("Product & Service")    → "product-and-service"
Slug.Generate("UPPERCASE Name")       → "uppercase-name"
Slug.Generate("Multiple   Spaces")    → "multiple-spaces"

// ❌ Invalid inputs:
Slug.Generate("")                     → null
Slug.Generate("!!!###")               → null (no valid chars)
```

**SEO Importance**:
```csharp
// 🌐 Real-World Usage:
var product = Product.Create(..., "Gaming Laptop", ...);
// URL: /products/gaming-laptop  ← Slug.Value used here

// Without Slug:
// URL: /products/123e4567-e89b-12d3  ← Bad for SEO!
```

---

#### 2.4 SEOMetadata Value Object

**Location**: `/src/Domain/ValueObjects/SEOMetadata.cs`

**Why It's a Value Object**:
- Combination of meta title, description, keywords
- No identity - same metadata is same metadata
- Validation rules for SEO best practices
- Immutable - replace entire metadata, don't modify

**Validation Rules**:
```csharp
// ✅ SEO Best Practices Enforced:
- MetaTitle: Max 60 characters (Google displays ~60 chars)
- MetaDescription: Max 160 characters (Google displays ~160 chars)
- MetaKeywords: Max 255 characters
- Auto-truncation with "..." if too long
```

**Smart Generation**:
```csharp
// ✅ Auto-generation from content:
var seo = SEOMetadata.FromContent(
    "Gaming Laptop Pro",
    "High-performance gaming laptop with RTX graphics..."
);
// seo.MetaTitle = "Gaming Laptop Pro"
// seo.MetaDescription = "High-performance gaming laptop with RTX graphics..."

// ✅ Manual specification:
var seo = SEOMetadata.Create(
    "Buy Gaming Laptop Pro | Best Price",
    "Shop the ultimate gaming laptop with RTX 4090...",
    "gaming laptop, RTX, high performance"
);
```

---

#### 2.5 ProductImages Value Object

**Location**: `/src/Domain/ValueObjects/ProductImages.cs`

**Why It's a Value Object**:
- Collection of image URLs without identity
- Validation rules (max 10 additional images, valid URLs)
- Immutable operations (add/remove return NEW instance)
- No need to track "which" image collection

**Business Rules**:
```csharp
// ✅ CORRECT: Immutable operations
var images1 = ProductImages.Create("main.jpg", new[] {"alt1.jpg"});
var images2 = images1.AddImage("alt2.jpg");
// images1 still has 1 additional image
// images2 has 2 additional images
// They are DIFFERENT value objects

// ✅ Validation:
ProductImages.Create("not-a-url", null);
// MainImageUrl = null (invalid URL rejected)

var images = ProductImages.Empty();
for (int i = 0; i < 15; i++) {
    images = images.AddImage($"img{i}.jpg");
}
// images.AdditionalImageUrls.Count == 10 (max enforced)
```

---

#### 2.6 Dimensions Value Object

**Location**: `/src/Domain/ValueObjects/Dimensions.cs`

**Why It's a Value Object**:
- Represents physical dimensions (L × W × H)
- No identity - 10×20×30 cm is same as any other 10×20×30 cm
- Validation (all dimensions must be positive)
- Parsing from strings for convenience

**Usage Examples**:
```csharp
// ✅ Creation:
var dims = Dimensions.Create(50m, 30m, 20m, "cm");
// dims.Length = 50, Width = 30, Height = 20, Unit = "cm"

// ✅ Parsing:
var dims = Dimensions.Parse("50x30x20 cm");
var dims = Dimensions.Parse("50 x 30 x 20 cm");
var dims = Dimensions.Parse("50X30X20");

// ✅ Business operations:
var volume = dims.CalculateVolume();  // 50 * 30 * 20 = 30,000 cm³

// ✅ String representations:
dims.ToString();         // "50 x 30 x 20 cm"
dims.ToCompactString();  // "50x30x20cm"
```

---

### 3. Aggregates

**Pattern Definition**: A cluster of domain objects that can be treated as a single unit. Always accessed through the aggregate root.

#### 3.1 Product Aggregate

**Location**: `/src/Domain/Entities/Product.cs`

**Aggregate Root**: Product entity

**Aggregate Boundary Includes**:
```
Product Aggregate
├── Product (Root Entity)
│   ├── Id (Identity)
│   ├── Name, Description (Primitive fields)
│   ├── Price (Money Value Object)
│   ├── CompareAtPrice (Money Value Object)
│   ├── CostPrice (Money Value Object)
│   ├── SKU (SKU Value Object)
│   ├── Slug (Slug Value Object)
│   ├── Images (ProductImages Value Object)
│   ├── Dimensions (Dimensions Value Object)
│   ├── SEOMetadata (SEOMetadata Value Object)
│   ├── Tags (Collection of strings)
│   ├── StockQuantity (Primitive)
│   └── Status (Enum)
└── CategoryId (Reference to Category Aggregate)
```

**Why This Boundary?**:

1. **Consistency Boundary**: All product properties must be consistent as a unit:
   - If price changes, compare-at-price might need adjustment
   - If product is published, all required fields must be valid
   - Stock changes affect availability calculations

2. **Transactional Boundary**: Product changes are atomic:
   ```csharp
   // ✅ CORRECT: Single transaction
   var product = await _unitOfWork.Products.GetByIdAsync(id);
   product.ChangePrice(newPrice, "admin");
   product.UpdateStock(50, "admin");
   await _unitOfWork.SaveChangesAsync();
   // Both changes committed together or rolled back together
   ```

3. **What's EXCLUDED and Why**:
   - **Category**: Separate aggregate (independent lifecycle)
   - **Order Lines**: Separate aggregate (orders are independent)
   - **Product Reviews**: Could be separate aggregate (different consistency needs)

**Invariants Protected**:
```csharp
// ✅ Enforced at Aggregate Root level:
1. Price Consistency Rule:
   product.SetCompareAtPrice(Money.Create(80m));
   // ❌ Throws if current price is 100 (compare-at must be > price)

2. Publishing Validation:
   product.Publish("admin");
   // ❌ Throws if: no name, invalid price, or no category

3. Stock Rules:
   product.UpdateStock(-10);
   // ❌ Throws: "Stock quantity cannot be negative"

4. Cost vs. Selling Price:
   product.SetCostPrice(Money.Create(150m));
   // ❌ Warns/Throws if selling price is 100
```

**Concurrency Considerations**:
```csharp
// ⚠️ Optimistic Concurrency Example:
// User A loads product (version 1)
var productA = await _repo.GetByIdAsync(id);
productA.ChangePrice(newPrice1);

// User B loads same product (version 1)
var productB = await _repo.GetByIdAsync(id);
productB.ChangePrice(newPrice2);

// User A saves (version 2)
await _unitOfWork.SaveChangesAsync();  // ✅ Success

// User B saves (still thinks version is 1)
await _unitOfWork.SaveChangesAsync();  // ❌ Concurrency exception!
// EF Core's UpdatedAt timestamp protects against this
```

**Anti-Pattern Avoided**:
```csharp
// 🚫 WRONG: Modifying through navigation property
var category = product.Category;
category.Name = "New Name";  // ❌ Crossing aggregate boundary!

// ✅ CORRECT: Modify through Category aggregate root
var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
category.UpdateInformation("New Name", desc, "admin");
```

---

#### 3.2 Category Aggregate

**Location**: `/src/Domain/Entities/Category.cs`

**Aggregate Root**: Category entity

**Aggregate Boundary Includes**:
```
Category Aggregate
├── Category (Root Entity)
│   ├── Id (Identity)
│   ├── Name, Description
│   ├── Slug (Slug Value Object)
│   ├── ParentCategoryId (Reference to parent Category)
│   ├── DisplayOrder
│   ├── IsActive
│   └── ImageUrl
└── Products (Reference only - NOT loaded in write operations)
```

**Why Category is a Separate Aggregate**:

1. **Independent Lifecycle**:
   ```csharp
   // Categories exist before products
   var category = Category.Create(tenantId, "Electronics", ...);

   // Categories continue after products deleted
   products.ForEach(p => _repo.Delete(p));
   // Category still exists and valid
   ```

2. **Different Consistency Needs**:
   ```csharp
   // ✅ Can modify category without locking all products
   category.UpdateInformation("Tech Devices", desc, "admin");
   // Doesn't require consistency check with 1000s of products
   ```

3. **Scalability**:
   ```csharp
   // ❌ WRONG: If Category contained all Products
   var category = await _repo.GetByIdAsync(categoryId);
   // Would load ALL products in memory! (could be 10,000s)

   // ✅ CORRECT: Separate aggregates
   var category = await _repo.GetByIdAsync(categoryId);
   // Only loads category (small, fast)
   ```

**Hierarchical Relationships**:
```csharp
// ✅ Parent-Child handled carefully:
var electronics = Category.Create(tenantId, "Electronics", ...);
var laptops = Category.Create(tenantId, "Laptops", ..., electronics.Id);

// Business Rule Enforcement:
laptops.ChangeParent(laptops.Id, "admin");
// ❌ Throws: "Category cannot be its own parent"

// ✅ Checking hierarchy:
if (laptops.IsRootCategory()) { } // false
if (electronics.IsRootCategory()) { } // true
```

**Navigation Property Trade-off**:
```csharp
// ⚠️ IMPORTANT: Products navigation property exists for queries
public IReadOnlyCollection<Product> Products { get; }

// ✅ CORRECT: Query usage (read-only)
var category = await _repo.GetWithProductsAsync(id);
var productCount = category.Products.Count;  // OK for display

// ❌ WRONG: Modifying across aggregate boundary
var category = await _repo.GetWithProductsAsync(id);
var product = category.Products.First();
product.ChangePrice(newPrice);  // ❌ Don't do this!

// ✅ CORRECT: Load product aggregate separately
var product = await _unitOfWork.Products.GetByIdAsync(productId);
product.ChangePrice(newPrice, "admin");  // Modify through aggregate root
```

---

### 4. Domain Events

**Pattern Definition**: Something that happened in the domain that domain experts care about. Used for decoupling and eventual consistency.

#### 4.1 ProductCreatedEvent

**Location**: `/src/Domain/Events/ProductCreatedEvent.cs`

**When Raised**: Immediately after a new Product aggregate is created via `Product.Create()`

**Business Significance**:
- New inventory item available
- Might need to update search index
- Might trigger welcome notifications
- Analytics tracking

**Code Example**:
```csharp
// ✅ Event raised in domain:
public static Product Create(...) {
    var product = new Product { ... };

    // Raise domain event
    product.AddDomainEvent(new ProductCreatedEvent(
        product.Id,
        tenantId,
        categoryId,
        product.Name,
        sku.Value
    ));

    return product;
}

// ✅ Event handler (in Application layer):
public class ProductCreatedEventHandler
    : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent @event, CancellationToken ct)
    {
        // Update search index
        await _searchService.IndexProduct(@event.ProductId);

        // Send analytics
        await _analytics.TrackEvent("ProductCreated", @event);

        // No direct dependencies in domain!
    }
}
```

**Why Not Just Call Methods Directly?**:
```csharp
// 🚫 WRONG: Direct coupling
public static Product Create(...) {
    var product = new Product { ... };

    // ❌ Domain depends on infrastructure!
    _searchService.IndexProduct(product.Id);
    _analytics.TrackEvent("ProductCreated");

    return product;
}

// ✅ CORRECT: Domain events decouple
public static Product Create(...) {
    var product = new Product { ... };

    // Domain only knows "something happened"
    product.AddDomainEvent(new ProductCreatedEvent(...));

    // Handlers registered elsewhere decide what to do
    return product;
}
```

---

#### 4.2 ProductPriceChangedEvent

**Location**: `/src/Domain/Events/ProductPriceChangedEvent.cs`

**When Raised**: When `product.ChangePrice()` is called

**Business Significance**:
- Price history tracking
- Notify subscribers of price drops
- Update materialized views
- Trigger repricing rules

**Eventual Consistency Example**:
```csharp
// ✅ Command: Change price
var product = await _unitOfWork.Products.GetByIdAsync(id);
product.ChangePrice(newPrice, "admin");
await _unitOfWork.SaveChangesAsync();
// Event dispatched: ProductPriceChangedEvent

// ✅ Handler 1: Update search index
public class UpdateSearchIndexHandler
    : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent @event, CancellationToken ct)
    {
        // Eventually consistent search index
        await _searchIndex.UpdatePrice(@event.ProductId, @event.NewPrice);
    }
}

// ✅ Handler 2: Notify users
public class PriceChangeNotificationHandler
    : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent @event, CancellationToken ct)
    {
        // Find users watching this product
        var watchers = await _watcherRepo.GetByProductId(@event.ProductId);

        // Send price drop notifications
        if (@event.NewPrice < @event.OldPrice)
        {
            await _notificationService.NotifyPriceDrop(watchers, @event);
        }
    }
}
```

**Key Learning**: Multiple handlers can respond to one event, each doing different things, without the domain knowing about any of them!

---

#### 4.3 ProductStockChangedEvent

**Location**: `/src/Domain/Events/ProductStockChangedEvent.cs`

**When Raised**: When `product.UpdateStock()`, `product.IncreaseStock()`, or `product.DecreaseStock()` is called

**Business Significance**:
- Low stock alerts
- Reorder point triggers
- Availability updates
- Analytics

**Real-World Example**:
```csharp
// ✅ Stock change triggers multiple workflows:
product.UpdateStock(5, "admin");  // Below low stock threshold!

// Event: ProductStockChangedEvent(oldQty: 50, newQty: 5, isLowStock: true)

// ✅ Handler 1: Low stock alert
public class LowStockAlertHandler
    : INotificationHandler<ProductStockChangedEvent>
{
    public async Task Handle(ProductStockChangedEvent @event, CancellationToken ct)
    {
        if (@event.IsLowStock)
        {
            await _alertService.SendLowStockAlert(@event.ProductId);
        }
    }
}

// ✅ Handler 2: Auto-reorder
public class AutoReorderHandler
    : INotificationHandler<ProductStockChangedEvent>
{
    public async Task Handle(ProductStockChangedEvent @event, CancellationToken ct)
    {
        if (@event.NewQuantity == 0)
        {
            await _purchaseService.CreateReorderRequest(@event.ProductId);
        }
    }
}

// ✅ Handler 3: Update product availability
public class UpdateAvailabilityHandler
    : INotificationHandler<ProductStockChangedEvent>
{
    public async Task Handle(ProductStockChangedEvent @event, CancellationToken ct)
    {
        var isAvailable = @event.NewQuantity > 0;
        await _cacheService.UpdateAvailability(@event.ProductId, isAvailable);
    }
}
```

---

#### 4.4 ProductPublishedEvent

**Location**: `/src/Domain/Events/ProductPublishedEvent.cs`

**When Raised**: When `product.Publish()` successfully changes status to Active

**Business Significance**:
- Product now visible to customers
- Update storefront cache
- Send to CDN
- Marketing notifications

**State Transition Example**:
```csharp
// ✅ Publishing workflow with events:

// 1. Product in Draft state
var product = Product.Create(...);
// product.Status == ProductStatus.Draft

// 2. Merchant adds all required info
product.UpdateInformation(...);
product.SetImages(...);
product.UpdateStock(100, "admin");

// 3. Merchant publishes
product.Publish("admin");
// Validates: ✅ has name, ✅ has price, ✅ has category
// Changes status: Draft → Active
// Raises: ProductPublishedEvent

// 4. Event handlers make it visible:
public class PublishToStorefrontHandler
    : INotificationHandler<ProductPublishedEvent>
{
    public async Task Handle(ProductPublishedEvent @event, CancellationToken ct)
    {
        // Add to search index
        await _searchService.PublishProduct(@event.ProductId);

        // Invalidate cache
        await _cache.InvalidateCategory(@event.TenantId);

        // Send to CDN
        await _cdn.PreloadImages(@event.ProductId);
    }
}
```

---

### 5. Repositories

**Pattern Definition**: Provides the illusion of an in-memory collection of aggregates. Mediates between domain and data mapping layers.

#### 5.1 IProductRepository

**Location**: `/src/Domain/Interfaces/IProductRepository.cs`

**Ubiquitous Language in Method Names**:
```csharp
// ✅ GOOD: Speaks business language
Task<Product?> GetBySkuAsync(string sku, Guid tenantId, ...);
Task<IReadOnlyList<Product>> GetFeaturedAsync(Guid tenantId, ...);
Task<bool> SkuExistsAsync(string sku, Guid tenantId, ...);

// ❌ BAD: Technical language
Task<Product?> SelectBySku(string sku, ...);  // "Select" is SQL
Task<IReadOnlyList<Product>> QueryFeatured(...);  // "Query" is technical
Task<bool> CheckSkuDuplicate(...);  // Unclear what "Check" means
```

**Aggregate Loading Strategy**:
```csharp
// ✅ Load aggregate root with value objects:
Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
// Returns: Product with all value objects (Money, SKU, Slug, etc.)
// Includes: CategoryId (reference only, not full Category)

// ✅ Load with navigation for queries:
Task<Product?> GetWithCategoryAsync(Guid id, CancellationToken ct);
// Returns: Product with Category loaded (for display purposes)
// Use: Read operations only, never modify across aggregates

// ❌ WRONG: Don't expose IQueryable
IQueryable<Product> GetAll();  // Breaks encapsulation
// Allows: repo.GetAll().Where(p => p.Price > 100)
// Problem: Domain logic (price filtering) leaks to application layer
```

**Why Separate Repositories for Each Aggregate**:
```csharp
// ✅ CORRECT: One repository per aggregate root
public interface IProductRepository { ... }
public interface ICategoryRepository { ... }

// ❌ WRONG: Generic repository
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    // Problem: Products and Categories have different loading needs
}

// ❌ WRONG: God repository
public interface IDataRepository
{
    Task<Product> GetProduct(Guid id);
    Task<Category> GetCategory(Guid id);
    Task<Order> GetOrder(Guid id);
    // Problem: Violates Single Responsibility Principle
}
```

---

#### 5.2 ICategoryRepository

**Location**: `/src/Domain/Interfaces/ICategoryRepository.cs`

**Domain-Specific Methods**:
```csharp
// ✅ Hierarchy-specific operations:
Task<IReadOnlyList<Category>> GetRootCategoriesAsync(Guid tenantId, ...);
Task<IReadOnlyList<Category>> GetChildCategoriesAsync(Guid parentId, ...);

// ✅ Load with products for display:
Task<Category?> GetWithProductsAsync(Guid id, ...);
// Note: Only use for queries, not commands!

// ✅ Business validation query:
Task<bool> SlugExistsAsync(string slug, Guid tenantId, ...);
```

**Performance Considerations**:
```csharp
// ⚠️ Be careful with GetWithProductsAsync:
var category = await _repo.GetWithProductsAsync(categoryId);
// Could load 10,000 products into memory!

// ✅ Better: Use pagination at application layer
var products = await _productRepo.GetByCategoryAsync(
    categoryId,
    skip: 0,
    take: 20
);
```

---

### 6. Factory Methods

**Pattern Definition**: Encapsulates complex object creation logic, ensuring objects are created in a valid state.

#### 6.1 Product.Create()

**Location**: `/src/Domain/Entities/Product.cs` (line 65)

**Why Use Factory Method**:
```csharp
// ❌ WRONG: Public constructor allows invalid state
public class Product
{
    public Product() { }  // Anyone can create empty product

    public string Name { get; set; }  // Could be null!
    public decimal Price { get; set; }  // Could be negative!
}

var product = new Product();  // ✅ Compiles
// product.Name is null
// product.Price is 0
// ❌ Invalid product created!

// ✅ CORRECT: Factory method enforces validity
public class Product
{
    private Product() { }  // Private! Can't use 'new'

    public static Product Create(
        Guid tenantId,
        Guid categoryId,
        string name,
        string? description,
        SKU sku,
        Money price,
        string createdBy)
    {
        ValidateName(name);  // ✅ Validation

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            _sku = sku,  // ✅ Required
            _price = price,  // ✅ Required
            _status = ProductStatus.Draft,  // ✅ Default state
            // ... all required fields set
        };

        product.AddDomainEvent(new ProductCreatedEvent(...));

        return product;  // ✅ Always valid!
    }
}

// Usage:
var sku = SKU.Create("PROD-001")!;
var price = Money.Create(99.99m)!;
var product = Product.Create(tenantId, categoryId, "Laptop", desc, sku, price, "admin");
// ✅ Guaranteed valid product with all invariants satisfied
```

**What Factory Method Does**:
1. ✅ **Validates input**: Name not empty, price positive
2. ✅ **Sets defaults**: Status=Draft, TrackInventory=true
3. ✅ **Generates derived values**: Slug from name, SEO from content
4. ✅ **Raises domain events**: ProductCreatedEvent
5. ✅ **Sets audit fields**: CreatedAt, CreatedBy
6. ✅ **Returns valid object**: All invariants satisfied

---

#### 6.2 Category.Create()

**Location**: `/src/Domain/Entities/Category.cs` (line 38)

**Parent-Child Validation**:
```csharp
public static Category Create(
    Guid tenantId,
    string name,
    string? description,
    Guid? parentCategoryId,  // Optional parent
    string createdBy)
{
    ValidateName(name);

    var category = new Category
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Name = name.Trim(),
        _slug = Slug.Generate(name) ?? throw new InvalidOperationException(...),
        ParentCategoryId = parentCategoryId,  // Can be null (root category)
        IsActive = true,  // Default active
        DisplayOrder = 0
    };

    category.SetCreatedAudit(createdBy);
    category.AddDomainEvent(new CategoryCreatedEvent(...));

    return category;
}

// ✅ Usage:
var rootCategory = Category.Create(tenantId, "Electronics", null, null, "admin");
// rootCategory.IsRootCategory() == true

var subCategory = Category.Create(tenantId, "Laptops", null, rootCategory.Id, "admin");
// subCategory.ParentCategoryId == rootCategory.Id
```

---

### 7. Domain Services

**Pattern Definition**: Operations that don't naturally belong to an entity or value object. Stateless operations that work with multiple aggregates.

**When to Use Domain Services**:
- ✅ Operation involves multiple aggregates
- ✅ Operation doesn't feel natural on any single entity
- ✅ Operation is about domain logic, not infrastructure

**When NOT to Use**:
- ❌ Logic belongs to an entity (put it there!)
- ❌ Logic is about infrastructure (put in Application layer)
- ❌ Just because an operation uses a repository (Application layer handles that)

**Example: Why We DON'T Have a PricingService**:
```csharp
// 🚫 WRONG: Domain service doing entity work
public class PricingService
{
    public void ChangePrice(Product product, Money newPrice)
    {
        product.Price = newPrice;  // This belongs in Product!
    }

    public decimal CalculateDiscount(Product product)
    {
        return product.CompareAtPrice - product.Price;  // This belongs in Product!
    }
}

// ✅ CORRECT: Logic in entity
public class Product
{
    public void ChangePrice(Money newPrice, string updatedBy)
    {
        _price = newPrice;
        AddDomainEvent(new ProductPriceChangedEvent(...));
    }

    public decimal? CalculateDiscountPercentage()
    {
        if (_compareAtPrice == null) return null;
        return ((_compareAtPrice.Amount - _price.Amount) / _compareAtPrice.Amount) * 100;
    }
}
```

**Example: When We MIGHT Need a Domain Service**:
```csharp
// ✅ Hypothetical: Cross-aggregate pricing rules
public class PromotionService
{
    // Operation involves multiple products and categories
    public Money CalculateBundlePrice(
        IReadOnlyList<Product> products,
        Category category,
        decimal discountPercentage)
    {
        // Complex logic involving multiple aggregates
        // Doesn't belong to Product or Category alone
        // This is a domain service!
    }
}

// Note: We don't have this in our current implementation
// because our pricing rules are simple enough to live in Product
```

---

## Architectural Patterns

### Layered Architecture

**Structure**:
```
┌─────────────────────────────────────┐
│  API Layer (Controllers)            │
│  - Thin controllers                 │
│  - HTTP concerns only               │
└────────────┬────────────────────────┘
             │ depends on ↓
┌────────────▼────────────────────────┐
│  Application Layer                  │
│  - Use cases (Command/Query handlers)│
│  - DTOs                             │
│  - Orchestration                    │
└────────────┬────────────────────────┘
             │ depends on ↓
┌────────────▼────────────────────────┐
│  Domain Layer                       │
│  - Entities                         │
│  - Value Objects                    │
│  - Domain Events                    │
│  - Repository Interfaces            │
│  - NO dependencies on other layers  │
└─────────────────────────────────────┘
             ↑ implemented by
┌────────────┴────────────────────────┐
│  Infrastructure Layer               │
│  - Repository Implementations       │
│  - EF Core                          │
│  - External Services                │
└─────────────────────────────────────┘
```

**Dependency Rule**:
- API → Application → Domain ← Infrastructure
- **Domain has NO dependencies** (pure business logic)
- Infrastructure implements Domain interfaces

---

### CQRS

**Pattern**: Separate read and write operations

**Implementation**:
```
Commands (Write)              Queries (Read)
─────────────────            ──────────────
CreateProductCommand         GetProductQuery
UpdateProductCommand         GetProductsQuery
DeleteProductCommand         SearchProductsQuery
     │                            │
     ↓                            ↓
CommandHandler              QueryHandler
- Validates                 - Reads
- Calls Domain              - Projects to DTO
- Saves via UnitOfWork      - No business logic
```

**Example**:
```csharp
// ✅ Command: Write operation with business logic
public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(...)
    {
        // Validate
        var sku = SKU.Create(command.SKU);
        if (sku == null) return Result.Failure("Invalid SKU");

        // Business logic
        var product = Product.Create(...);
        product.UpdateStock(command.StockQuantity, "admin");

        // Save
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();  // Dispatches events

        return Result.Success(MapToDto(product));
    }
}

// ✅ Query: Read operation, no business logic
public class GetProductQueryHandler
    : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(...)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(query.Id);
        if (product == null) return Result.Failure("Not found");

        return Result.Success(MapToDto(product));  // Simple projection
    }
}
```

---

### Unit of Work

**Pattern**: Maintains a list of objects affected by a business transaction and coordinates writing changes.

**Location**: `/src/Infrastructure/Persistence/UnitOfWork.cs`

**Why Use It**:
```csharp
// ❌ WITHOUT Unit of Work:
await _productRepository.SaveChangesAsync();
await _categoryRepository.SaveChangesAsync();
// Problem: Two separate transactions! Could be inconsistent.

// ✅ WITH Unit of Work:
_unitOfWork.Products.Update(product);
_unitOfWork.Categories.Update(category);
await _unitOfWork.SaveChangesAsync();  // Single transaction!
```

**Domain Event Dispatching**:
```csharp
public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    // 1. Get all domain events
    var domainEvents = _context.ChangeTracker
        .Entries<BaseEntity>()
        .SelectMany(x => x.Entity.DomainEvents)
        .ToList();

    // 2. Clear events from entities
    domainEvents.ForEach(e => e.Entity.ClearDomainEvents());

    // 3. Save changes
    var result = await _context.SaveChangesAsync(ct);

    // 4. Dispatch events
    foreach (var domainEvent in domainEvents)
    {
        await _mediator.Publish(domainEvent, ct);
    }

    return result;
}
```

---

## Code Examples

### Example 1: Creating a Product (Full Flow)

```csharp
// Application Layer (Command Handler)
public async Task<Result<ProductDto>> Handle(CreateProductCommand cmd, CancellationToken ct)
{
    // 1. Validate category exists (cross-aggregate check)
    var category = await _unitOfWork.Categories.GetByIdAsync(cmd.CategoryId, ct);
    if (category == null)
        return Result.Failure("Category not found");

    // 2. Create value objects
    var sku = SKU.Create(cmd.SKU);
    if (sku == null)
        return Result.Failure("Invalid SKU format");

    var price = Money.Create(cmd.Price);
    if (price == null)
        return Result.Failure("Invalid price");

    // 3. Use factory method (Domain Layer)
    var product = Product.Create(
        cmd.TenantId,
        cmd.CategoryId,
        cmd.Name,
        cmd.Description,
        sku,
        price,
        "admin"
    );
    // ProductCreatedEvent raised here!

    // 4. Use business methods
    product.UpdateStock(cmd.StockQuantity, "admin");
    // ProductStockChangedEvent raised!

    if (cmd.Status == ProductStatus.Active)
    {
        product.Publish("admin");
        // ProductPublishedEvent raised!
    }

    // 5. Save via Unit of Work
    await _unitOfWork.Products.AddAsync(product, ct);
    await _unitOfWork.SaveChangesAsync(ct);
    // All 3 events dispatched here!

    // 6. Map to DTO
    return Result.Success(MapToDto(product, category));
}
```

---

### Example 2: Domain Event Flow

```csharp
// 1. Domain Layer: Event raised
product.ChangePrice(newPrice, "admin");
// Adds: ProductPriceChangedEvent(productId, oldPrice, newPrice)

// 2. Infrastructure: Event dispatched
await _unitOfWork.SaveChangesAsync();
// Publishes event via MediatR

// 3. Application Layer: Multiple handlers respond
public class UpdateSearchIndexHandler
    : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent e, CancellationToken ct)
    {
        await _searchService.UpdatePrice(e.ProductId, e.NewPrice);
    }
}

public class NotifyWatchersHandler
    : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent e, CancellationToken ct)
    {
        if (e.NewPrice < e.OldPrice)  // Price drop!
        {
            var watchers = await _repo.GetWatchers(e.ProductId);
            await _notificationService.Notify(watchers, e);
        }
    }
}

// 4. All handlers run independently
// If UpdateSearchIndexHandler fails, NotifyWatchersHandler still runs
// Domain doesn't know or care about any of this!
```

---

## Summary

This catalog service demonstrates **all major DDD tactical patterns**:

✅ **Entities** (Product, Category) with identity and lifecycle
✅ **Value Objects** (Money, SKU, Slug, etc.) for immutable concepts
✅ **Aggregates** with clear boundaries and consistency rules
✅ **Domain Events** for decoupling and eventual consistency
✅ **Repositories** speaking ubiquitous language
✅ **Factory Methods** ensuring valid object creation
✅ **Layered Architecture** with proper dependency direction
✅ **CQRS** separating reads and writes
✅ **Unit of Work** coordinating transactions

Each pattern solves specific problems and works together to create a maintainable, testable, business-focused codebase.

---

**Next**: See [AGGREGATE-DESIGN-DECISIONS.md](./AGGREGATE-DESIGN-DECISIONS.md) for deep dive into aggregate boundaries.
