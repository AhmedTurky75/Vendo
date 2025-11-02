# DDD Anti-Patterns Avoided

## Table of Contents
- [Introduction](#introduction)
- [Domain Layer Anti-Patterns](#domain-layer-anti-patterns)
- [Aggregate Anti-Patterns](#aggregate-anti-patterns)
- [Application Layer Anti-Patterns](#application-layer-anti-patterns)
- [Repository Anti-Patterns](#repository-anti-patterns)
- [Value Object Anti-Patterns](#value-object-anti-patterns)
- [Architecture Anti-Patterns](#architecture-anti-patterns)
- [Warning Signs](#warning-signs)

---

## Introduction

This document catalogs **common DDD mistakes** we deliberately avoided in the Catalog Service. Each anti-pattern includes:

1. **What it is** - Description of the anti-pattern
2. **Why it's bad** - Problems it causes
3. **How we avoided it** - Our solution
4. **Warning signs** - How to detect if you're falling into this trap

**Purpose:** Learn from others' mistakes before making them yourself!

---

## Domain Layer Anti-Patterns

### Anti-Pattern 1: Anemic Domain Model

**What It Is:**

Domain entities that are just data containers with getters/setters, no business logic.

**Example of Anti-Pattern:**
```csharp
// ❌ ANEMIC: No behavior, just data
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
}

// Business logic lives in services
public class ProductService
{
    public async Task ChangePrice(Guid productId, decimal newPrice)
    {
        var product = await _repository.GetByIdAsync(productId);

        // Validation in service layer (wrong!)
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative");

        product.Price = newPrice; // Direct setter (wrong!)
        await _repository.SaveChangesAsync();
    }
}
```

**Why It's Bad:**

1. **Business rules scattered** across service layer
2. **Easy to bypass validation** - anyone can set `product.Price = -100`
3. **Domain knowledge leaks** outside domain layer
4. **Hard to test** - need to test services, not domain
5. **Violates encapsulation** - internal state exposed

**How We Avoided It:**

```csharp
// ✅ RICH DOMAIN MODEL: Behavior + data
public class Product : BaseEntity
{
    // Private backing field
    private Money _price;

    // Read-only property
    public Money Price => _price;

    // Private constructor prevents invalid creation
    private Product() { }

    // Factory method enforces valid creation
    public static Product Create(...)
    {
        ValidateName(name);
        var product = new Product { ... };
        product.AddDomainEvent(new ProductCreatedEvent(...));
        return product;
    }

    // Business method encapsulates logic
    public void ChangePrice(Money newPrice, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        // Business rule: Clear compare-at price if no longer valid
        if (_compareAtPrice != null && !_compareAtPrice.IsGreaterThan(newPrice))
            _compareAtPrice = null;

        _price = newPrice;
        UpdateAudit(updatedBy);
        AddDomainEvent(new ProductPriceChangedEvent(Id, _price.Amount));
    }
}
```

**Benefits:**
- ✅ Business logic in domain entities
- ✅ Impossible to create invalid state
- ✅ Easy to test (just test entity methods)
- ✅ Self-documenting (methods show what's possible)

**Warning Signs:**
- Public setters everywhere
- All business logic in service classes
- Entities are just DTOs with an ID
- Method names like `SetPrice()` instead of `ChangePrice()`

---

### Anti-Pattern 2: Primitive Obsession

**What It Is:**

Using primitive types (string, decimal, int) instead of value objects for domain concepts.

**Example of Anti-Pattern:**
```csharp
// ❌ PRIMITIVES EVERYWHERE
public class Product
{
    public string SKU { get; set; }            // Just a string
    public decimal Price { get; set; }         // Just a number
    public string Currency { get; set; }       // Separate from price
    public string ImageUrl1 { get; set; }      // Messy
    public string ImageUrl2 { get; set; }
    public string ImageUrl3 { get; set; }
}

// Validation scattered everywhere
public class ProductService
{
    public void UpdatePrice(Product product, decimal price, string currency)
    {
        // Validation must be repeated everywhere
        if (price < 0) throw new Exception("Invalid price");
        if (string.IsNullOrEmpty(currency)) throw new Exception("Invalid currency");
        if (currency.Length != 3) throw new Exception("Currency must be 3 letters");

        product.Price = price;
        product.Currency = currency;
    }
}
```

**Why It's Bad:**

1. **No validation at creation** - can have `Price = -100`, `Currency = "xyz"`
2. **Validation duplicated** everywhere primitives are used
3. **Related data separated** - Price and Currency should be together
4. **No domain meaning** - `string SKU` doesn't convey business rules
5. **Unclear operations** - How do you add two prices? Just `price1 + price2`?

**How We Avoided It:**

```csharp
// ✅ VALUE OBJECTS
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Validation at creation - invalid Money cannot exist!
    public static Money? Create(decimal amount, string currency = "USD")
    {
        if (amount < 0) return null;  // Negative money is invalid
        if (string.IsNullOrEmpty(currency)) return null;

        return new Money(amount, currency.ToUpperInvariant());
    }

    // Domain operations
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    // Comparison with business meaning
    public bool IsGreaterThan(Money other) => Amount > other.Amount;
}

// Product uses value objects
public class Product : BaseEntity
{
    private Money _price;             // Not decimal
    private SKU _sku;                 // Not string
    private ProductImages? _images;   // Not multiple string properties

    public Money Price => _price;
    public SKU SKU => _sku;
}
```

**Benefits:**
- ✅ Validation centralized in value object
- ✅ Impossible to create invalid value (Money with negative amount can't exist)
- ✅ Domain operations built-in (`money1.Add(money2)`)
- ✅ Related data grouped (Amount + Currency together)

**Warning Signs:**
- Lots of validation for the same primitive type scattered across code
- Related primitives that always change together (price + currency)
- Business concepts represented as strings/decimals/ints
- Unclear what operations are valid (can you add SKUs?)

---

### Anti-Pattern 3: Public Setters on Entities

**What It Is:**

Entities with public setters allowing anyone to modify state without business logic.

**Example of Anti-Pattern:**
```csharp
// ❌ PUBLIC SETTERS
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }           // Anyone can change
    public decimal Price { get; set; }         // No validation
    public ProductStatus Status { get; set; }  // Can bypass business rules
}

// Elsewhere in code...
product.Price = -1000;  // Nothing prevents this!
product.Status = ProductStatus.Active;  // Even if product has no images!
```

**Why It's Bad:**

1. **Bypasses business logic** - anyone can change state
2. **No validation** - invalid states possible
3. **No audit trail** - who changed what?
4. **No events** - can't react to changes
5. **Breaks encapsulation** - internal state exposed

**How We Avoided It:**

```csharp
// ✅ PRIVATE SETTERS + BUSINESS METHODS
public class Product : BaseEntity
{
    private Money _price;
    private ProductImages? _images;

    // Read-only public property
    public Money Price => _price;
    public ProductStatus Status { get; private set; }  // Private setter!

    // Must use business method to change state
    public void ChangePrice(Money newPrice, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        // Business rules enforced
        if (_compareAtPrice != null && !_compareAtPrice.IsGreaterThan(newPrice))
            _compareAtPrice = null;

        _price = newPrice;
        UpdateAudit(updatedBy);  // Audit trail!
        AddDomainEvent(new ProductPriceChangedEvent(Id, _price.Amount));  // Event!
    }

    public void Publish(string updatedBy)
    {
        // Business rule: Cannot publish without images
        if (_images == null || !_images.Urls.Any())
            throw new InvalidOperationException("Cannot publish product without images");

        Status = ProductStatus.Active;  // Only this method can activate
        UpdateAudit(updatedBy);
        AddDomainEvent(new ProductPublishedEvent(Id, Name));
    }
}
```

**Benefits:**
- ✅ Business rules always enforced
- ✅ Audit trail captured (updatedBy parameter)
- ✅ Domain events raised automatically
- ✅ Impossible to bypass validation
- ✅ Clear API (method names show intent)

**Warning Signs:**
- Entities with `{ get; set; }` everywhere
- No business methods, just setters
- Validation code scattered across application layer
- No audit trail for entity changes

---

### Anti-Pattern 4: Missing Domain Events

**What It Is:**

Not raising events when important business events occur.

**Example of Anti-Pattern:**
```csharp
// ❌ NO EVENTS
public class Product
{
    public void ChangePrice(Money newPrice)
    {
        _price = newPrice;
        // That's it - no one knows price changed!
    }
}

// Application layer must manually trigger side effects
public class UpdateProductHandler
{
    public async Task Handle(UpdateProductCommand cmd)
    {
        var product = await _repo.GetByIdAsync(cmd.Id);
        product.ChangePrice(cmd.NewPrice);
        await _repo.SaveChangesAsync();

        // Manually trigger side effects (bad!)
        await _priceHistoryService.RecordPriceChange(product.Id, cmd.NewPrice);
        await _searchIndexer.UpdateProductPrice(product.Id, cmd.NewPrice);
        await _cacheInvalidator.InvalidateProduct(product.Id);
        // What if we forget one? What if we need to add more?
    }
}
```

**Why It's Bad:**

1. **Side effects scattered** across application layer
2. **Easy to forget** to trigger side effects
3. **Tight coupling** - command handler knows about all side effects
4. **Hard to add new reactions** - must modify existing code
5. **Can't test in isolation** - side effects mixed with main logic

**How We Avoided It:**

```csharp
// ✅ RAISE DOMAIN EVENTS
public class Product : BaseEntity
{
    public void ChangePrice(Money newPrice, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (_compareAtPrice != null && !_compareAtPrice.IsGreaterThan(newPrice))
            _compareAtPrice = null;

        _price = newPrice;
        UpdateAudit(updatedBy);

        // Raise event - don't handle side effects here!
        AddDomainEvent(new ProductPriceChangedEvent(Id, _price.Amount));
    }
}

// Base entity manages events
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Unit of Work dispatches events
public class UnitOfWork : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        await DispatchDomainEventsAsync(ct);  // Automatic!
        return await _context.SaveChangesAsync(ct);
    }
}

// Handlers react to events (decoupled!)
public class ProductPriceChangedHandler : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent evt)
    {
        await _priceHistoryService.RecordPriceChange(evt.ProductId, evt.NewPrice);
    }
}

public class UpdateSearchIndexHandler : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent evt)
    {
        await _searchIndexer.UpdateProductPrice(evt.ProductId, evt.NewPrice);
    }
}
```

**Benefits:**
- ✅ Side effects decoupled from main logic
- ✅ Easy to add new reactions (just add handler)
- ✅ Events dispatched automatically by UnitOfWork
- ✅ Testable in isolation
- ✅ Clear communication of business events

**Warning Signs:**
- Service methods with long lists of side-effect calls
- Comments like "TODO: Remember to update cache"
- Hard to add new integrations without modifying existing code
- No way to know what happens when business event occurs

---

## Aggregate Anti-Patterns

### Anti-Pattern 5: Aggregate Contains Other Aggregates

**What It Is:**

An aggregate directly containing other aggregate instances (not just IDs).

**Example of Anti-Pattern:**
```csharp
// ❌ PRODUCT CONTAINS CATEGORY AGGREGATE
public class Product : BaseEntity
{
    public Category Category { get; set; }  // Full object, not just ID!

    public void ChangeCategory(Category newCategory)
    {
        Category = newCategory;
    }
}

// Loading product loads entire category
var product = await _context.Products
    .Include(p => p.Category)  // Forced to load category!
    .FirstAsync(p => p.Id == productId);
```

**Why It's Bad:**

1. **Forces loading related aggregates** - performance hit
2. **Breaks transaction boundaries** - updating product might lock category
3. **Duplicates data** - multiple products have separate Category copies
4. **Consistency issues** - if category changes, all product copies outdated
5. **Violates aggregate independence**

**How We Avoided It:**

```csharp
// ✅ REFERENCE BY ID ONLY
public class Product : BaseEntity
{
    // Only store the ID, not the object
    public Guid CategoryId { get; private set; }

    public void ChangeCategory(Guid newCategoryId, string updatedBy)
    {
        // Application layer verifies category exists
        // Domain just stores the reference
        CategoryId = newCategoryId;
        UpdateAudit(updatedBy);
    }
}

// Loading product doesn't load category
var product = await _context.Products
    .FirstAsync(p => p.Id == productId);  // Only product data

// Load category separately if needed
var category = await _context.Categories
    .FirstAsync(c => c.Id == product.CategoryId);

// OR use query model (CQRS read side)
var dto = await _context.Products
    .Where(p => p.Id == productId)
    .Select(p => new ProductDisplayDto
    {
        Id = p.Id,
        Name = p.Name,
        CategoryName = p.Category.Name  // Join in query, not in domain
    })
    .FirstAsync();
```

**Benefits:**
- ✅ Aggregates load independently
- ✅ No forced joins in domain operations
- ✅ Clear transaction boundaries
- ✅ No data duplication
- ✅ Aggregates can scale separately

**Warning Signs:**
- Navigation properties to other aggregates
- Loading one aggregate always loads others
- `.Include()` everywhere in repository queries
- Circular references between aggregates

---

### Anti-Pattern 6: Aggregate Too Large

**What It Is:**

Aggregates that contain too many entities/value objects, causing performance and concurrency issues.

**Example of Anti-Pattern:**
```csharp
// ❌ GIANT AGGREGATE
public class Catalog : BaseEntity
{
    // Aggregate includes EVERYTHING
    private List<Category> _categories = new();      // Could be 1000s
    private List<Product> _products = new();         // Could be 10,000s
    private List<Review> _reviews = new();           // Could be 100,000s

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    public void AddProduct(Product product)
    {
        _products.Add(product);
    }
}

// Loading catalog is a nightmare
var catalog = await _repo.GetCatalogAsync(tenantId);
// Loads 10,000+ products, 1,000+ categories, 100,000+ reviews!
```

**Why It's Bad:**

1. **Terrible performance** - loading aggregate loads everything
2. **Memory exhaustion** - might not fit in memory
3. **Concurrency hell** - every operation locks entire aggregate
4. **Violates aggregate rules** - not a single consistency boundary
5. **Can't scale** - everything in one transaction

**How We Avoided It:**

```csharp
// ✅ SMALL, FOCUSED AGGREGATES
public class Product : BaseEntity
{
    // Only includes what MUST be consistent together
    private SKU _sku;
    private Money _price;
    private Money? _compareAtPrice;
    private ProductImages? _images;
    private SEOMetadata? _seoMetadata;
    private Dimensions? _dimensions;
    private List<string> _tags = new();

    // Total: ~10-20 properties max
}

public class Category : BaseEntity
{
    // Separate aggregate, separate consistency boundary
    private string _name;
    private Slug _slug;
    private SEOMetadata? _seoMetadata;

    // Total: ~8 properties
}

// Products and Categories are separate
// Reviews would be separate aggregate in Review service
```

**Rule of Thumb:**
- Keep aggregates < 1000 lines of code
- Keep to single responsibility
- If loading is slow (>100ms), aggregate might be too large

**Benefits:**
- ✅ Fast loading
- ✅ Better concurrency (smaller locks)
- ✅ Clear boundaries
- ✅ Easier to understand and maintain

**Warning Signs:**
- Loading aggregate takes >100ms
- Aggregate class is >1000 lines
- Collections of child entities with 100s/1000s of items
- Many unrelated business methods
- Frequent merge conflicts on aggregate class

---

### Anti-Pattern 7: Navigating Across Aggregates

**What It Is:**

Walking from one aggregate to another through object references.

**Example of Anti-Pattern:**
```csharp
// ❌ NAVIGATION BETWEEN AGGREGATES
public class Product
{
    public Category Category { get; set; }  // Navigation property
}

public class Category
{
    public Category ParentCategory { get; set; }  // Navigation property
    public List<Product> Products { get; set; }   // Collection navigation
}

// Code walks the graph
var product = await _repo.GetByIdAsync(productId);
var category = product.Category;                    // Navigate to aggregate
var parent = category.ParentCategory;               // Navigate again
var siblings = parent.Products;                      // Load all products!
```

**Why It's Bad:**

1. **Infinite chains** - where does loading stop?
2. **Performance nightmare** - lazy loading hell or huge eager loads
3. **Breaks aggregate boundaries** - treats separate aggregates as one
4. **Unpredictable behavior** - depends on ORM configuration

**How We Avoided It:**

```csharp
// ✅ NO NAVIGATION PROPERTIES BETWEEN AGGREGATES
public class Product : BaseEntity
{
    // Only ID reference
    public Guid CategoryId { get; private set; }

    // NO navigation property!
    // public Category Category { get; set; }  ❌
}

public class Category : BaseEntity
{
    // Only ID reference to parent
    public Guid? ParentCategoryId { get; private set; }

    // NO navigation properties!
    // public Category ParentCategory { get; set; }  ❌
    // public List<Product> Products { get; set; }   ❌
}

// Load aggregates through repositories
var product = await _unitOfWork.Products.GetByIdAsync(productId);
var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

// For hierarchies, use explicit methods
public class CategoryRepository
{
    public async Task<List<Category>> GetCategoryPathAsync(Guid categoryId)
    {
        var path = new List<Category>();
        var current = await GetByIdAsync(categoryId);

        while (current != null)
        {
            path.Insert(0, current);
            current = current.ParentCategoryId.HasValue
                ? await GetByIdAsync(current.ParentCategoryId.Value)
                : null;
        }

        return path;
    }
}
```

**Benefits:**
- ✅ Explicit loading - know exactly what's loaded
- ✅ No surprise queries
- ✅ Respects aggregate boundaries
- ✅ Predictable performance

**Warning Signs:**
- Chains like `product.Category.Parent.Parent.Name`
- Lazy loading proxies everywhere
- `Include().ThenInclude().ThenInclude()` chains
- Not sure how much data a query loads

---

## Application Layer Anti-Patterns

### Anti-Pattern 8: Business Logic in Command Handlers

**What It Is:**

Command handlers contain business rules instead of just orchestrating domain operations.

**Example of Anti-Pattern:**
```csharp
// ❌ BUSINESS LOGIC IN HANDLER
public class UpdateProductCommandHandler
{
    public async Task Handle(UpdateProductCommand request)
    {
        var product = await _repo.GetByIdAsync(request.Id);

        // Business rules in handler (WRONG!)
        if (request.Price < 0)
            return Result.Failure("Price cannot be negative");

        if (request.CompareAtPrice != null && request.CompareAtPrice <= request.Price)
            return Result.Failure("Compare-at price must be greater than price");

        // Directly setting properties (WRONG!)
        product.Name = request.Name;
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;

        await _repo.SaveChangesAsync();
        return Result.Success();
    }
}
```

**Why It's Bad:**

1. **Domain knowledge outside domain** - harder to maintain
2. **Difficult to test** - must test handlers instead of domain
3. **Duplicate validation** - same rules in multiple handlers
4. **Can bypass rules** - just set properties directly
5. **Anemic domain model** - entities become data bags

**How We Avoided It:**

```csharp
// ✅ BUSINESS LOGIC IN DOMAIN
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(UpdateProductCommand request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product == null)
            return Result.Failure("Product not found");

        // Create value objects (validation in value object!)
        var newPrice = Money.Create(request.Price);
        if (newPrice == null)
            return Result.Failure("Invalid price");

        // Call domain method (validation in domain!)
        product.ChangePrice(newPrice, request.UpdatedBy);

        if (request.CompareAtPrice.HasValue)
        {
            var compareAt = Money.Create(request.CompareAtPrice.Value);
            if (compareAt == null)
                return Result.Failure("Invalid compare-at price");

            // Domain enforces business rule!
            try
            {
                product.SetCompareAtPrice(compareAt, request.UpdatedBy);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        // Just orchestration - domain has all business logic
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}
```

**Benefits:**
- ✅ Business logic centralized in domain
- ✅ Easy to test (test domain methods)
- ✅ Handler just orchestrates
- ✅ Can't bypass validation

**Warning Signs:**
- Lots of `if` statements in handlers
- Validation logic in handlers
- Direct property setters instead of method calls
- Handlers with complex business rules

---

### Anti-Pattern 9: Fat DTOs Matching Database Schema

**What It Is:**

DTOs that expose entire database schema instead of use-case-specific data.

**Example of Anti-Pattern:**
```csharp
// ❌ FAT DTO WITH EVERYTHING
public class ProductDto
{
    // Includes EVERYTHING from database
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public ProductStatus Status { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsActive { get; set; }
    public List<string> ImageUrls { get; set; }
    public List<string> Tags { get; set; }
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public string MetaKeywords { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string DimensionUnit { get; set; }
    public decimal? Weight { get; set; }
    public string WeightUnit { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    // ... 50+ properties!
}

// Used for EVERYTHING
public Task<ProductDto> GetForDisplay(Guid id);
public Task<ProductDto> GetForEdit(Guid id);
public Task<ProductDto> GetForSearch(Guid id);
```

**Why It's Bad:**

1. **Over-fetching** - returns data client doesn't need
2. **Security risk** - might expose sensitive data
3. **Performance** - transferring unnecessary data
4. **Unclear intent** - what does client actually need?
5. **Tight coupling** - DTO matches database, not use case

**How We Avoided It:**

```csharp
// ✅ USE-CASE-SPECIFIC DTOs
public class ProductListItemDto  // For product listing
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string PrimaryImageUrl { get; set; }  // Just first image
    public bool InStock { get; set; }
}

public class ProductDetailDto  // For product detail page
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public List<string> ImageUrls { get; set; }
    public List<string> Tags { get; set; }
    public bool InStock { get; set; }
    public string CategoryName { get; set; }
    // Only what's shown on detail page
}

public class ProductEditDto  // For admin edit form
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SKU { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public List<string> ImageUrls { get; set; }
    public List<string> Tags { get; set; }
    // Only editable fields
}
```

**Benefits:**
- ✅ Only transfer needed data
- ✅ Clear intent (DTO name shows use case)
- ✅ Better security (expose only what's needed)
- ✅ Faster (less data transfer)

**Warning Signs:**
- One DTO used everywhere
- DTOs with 30+ properties
- Clients asking "what does this field mean?"
- Security reviews finding exposed sensitive data

---

## Repository Anti-Patterns

### Anti-Pattern 10: Generic Repository Pattern

**What It Is:**

A single generic `IRepository<T>` for all entities instead of aggregate-specific repositories.

**Example of Anti-Pattern:**
```csharp
// ❌ GENERIC REPOSITORY
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// Used for everything
IRepository<Product> productRepo;
IRepository<Category> categoryRepo;
IRepository<Order> orderRepo;
```

**Why It's Bad:**

1. **Leaks implementation details** - exposes `FindAsync` with predicates
2. **Bypasses domain** - can query anything without business logic
3. **Unclear ubiquitous language** - not domain-specific
4. **Encourages specification pattern abuse**
5. **One size fits all** - doesn't match aggregate needs

**How We Avoided It:**

```csharp
// ✅ AGGREGATE-SPECIFIC REPOSITORIES
public interface IProductRepository
{
    // Methods match domain language
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<List<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<List<Product>> GetLowStockProductsAsync(CancellationToken ct = default);
    Task<List<Product>> GetByTagAsync(string tag, CancellationToken ct = default);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Remove(Product product);
}

public interface ICategoryRepository
{
    // Different methods - matches Category aggregate needs
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<Category>> GetTopLevelCategoriesAsync(CancellationToken ct = default);
    Task<List<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Remove(Category category);
}
```

**Benefits:**
- ✅ Methods match domain language
- ✅ Each aggregate has specific queries it needs
- ✅ Clear intent (method names are self-documenting)
- ✅ Can optimize per aggregate
- ✅ No leaking of query syntax to application layer

**Warning Signs:**
- `IRepository<T>` everywhere
- Application layer building complex LINQ queries
- No domain-specific query methods
- Method names like `Find()` instead of `GetLowStockProducts()`

---

### Anti-Pattern 11: Repositories with SaveChanges

**What It Is:**

Each repository has its own `SaveChangesAsync()` method instead of using Unit of Work.

**Example of Anti-Pattern:**
```csharp
// ❌ REPOSITORY WITH SAVE
public interface IProductRepository
{
    Task<Product> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task SaveChangesAsync();  // ❌ Each repo has this!
}

public interface ICategoryRepository
{
    Task<Category> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task SaveChangesAsync();  // ❌ Duplicate!
}

// Usage - updating multiple aggregates is unclear
var product = await _productRepo.GetByIdAsync(productId);
product.ChangeName("New Name");
await _productRepo.SaveChangesAsync();  // Saves product

var category = await _categoryRepo.GetByIdAsync(categoryId);
category.ChangeName("New Name");
await _categoryRepo.SaveChangesAsync();  // Saves category

// What if we want to save both in one transaction?
```

**Why It's Bad:**

1. **No transaction coordination** - can't update multiple aggregates atomically
2. **Duplicate code** - SaveChanges in every repository
3. **No domain event coordination** - events dispatched separately
4. **Unclear transaction boundaries**

**How We Avoided It:**

```csharp
// ✅ REPOSITORIES WITHOUT SAVE
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Remove(Product product);
    // NO SaveChangesAsync!
}

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Remove(Category category);
    // NO SaveChangesAsync!
}

// ✅ UNIT OF WORK COORDINATES SAVES
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);  // Single save point!
}

// Usage - clear transaction boundary
public class UpdateProductCommandHandler
{
    public async Task Handle(UpdateProductCommand request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        product.ChangeName(request.Name);

        // Single save - dispatches all events, one transaction
        await _unitOfWork.SaveChangesAsync();
    }
}
```

**Benefits:**
- ✅ Clear transaction boundaries
- ✅ Events dispatched atomically
- ✅ Easy to coordinate multiple aggregates (when needed)
- ✅ Single point of save

**Warning Signs:**
- `SaveChangesAsync()` in every repository
- Unclear when changes are persisted
- Multiple save calls in one handler
- Domain events firing at different times

---

## Value Object Anti-Patterns

### Anti-Pattern 12: Mutable Value Objects

**What It Is:**

Value objects with setters, allowing modification after creation.

**Example of Anti-Pattern:**
```csharp
// ❌ MUTABLE VALUE OBJECT
public class Money
{
    public decimal Amount { get; set; }  // Setter!
    public string Currency { get; set; } // Setter!

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}

// Can be changed after creation
var price = new Money(100, "USD");
price.Amount = -1000;  // Nothing prevents this!
price.Currency = "INVALID";  // Or this!
```

**Why It's Bad:**

1. **Can become invalid** after creation
2. **Breaks value equality** - if two Money instances with same value become unequal after mutation
3. **Side effects** - changing shared value affects multiple owners
4. **Thread safety** - mutable = not thread-safe

**How We Avoided It:**

```csharp
// ✅ IMMUTABLE VALUE OBJECT
public sealed class Money : ValueObject, IEquatable<Money>
{
    // Read-only properties (no setters!)
    public decimal Amount { get; }
    public string Currency { get; }

    // Private constructor - can't create directly
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Factory method validates before creation
    public static Money? Create(decimal amount, string currency = "USD")
    {
        if (amount < 0) return null;
        if (string.IsNullOrEmpty(currency)) return null;

        return new Money(amount, currency.ToUpperInvariant());
    }

    // Operations return NEW instances
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    // Value equality
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

**Benefits:**
- ✅ Cannot become invalid
- ✅ Thread-safe
- ✅ Can be safely shared
- ✅ Value equality guaranteed

**Warning Signs:**
- Value objects with `{ get; set; }`
- Value objects that can be modified after creation
- No validation in constructor
- Not overriding `Equals()` and `GetHashCode()`

---

### Anti-Pattern 13: Value Objects with Identity

**What It Is:**

Value objects that have IDs or are treated as entities.

**Example of Anti-Pattern:**
```csharp
// ❌ VALUE OBJECT WITH ID
public class Money : BaseEntity  // Inherits from entity!
{
    public Guid Id { get; set; }  // Value object shouldn't have ID!
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

// Stored in separate table
public class Product
{
    public Guid PriceId { get; set; }  // Foreign key to Money table
    public Money Price { get; set; }
}
```

**Why It's Bad:**

1. **Violates value object definition** - value objects don't have identity
2. **Unnecessary complexity** - separate table for values
3. **Wrong equality** - compared by ID instead of value
4. **Can't share** - each product needs separate Money instance

**How We Avoided It:**

```csharp
// ✅ VALUE OBJECT WITHOUT IDENTITY
public sealed class Money : ValueObject, IEquatable<Money>
{
    // NO Id property!
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Equality by VALUE, not ID
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

// EF Core configuration - owned type, not separate table
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Money is owned - stored in same table as Product
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });
    }
}
```

**Benefits:**
- ✅ True value semantics
- ✅ Compared by value
- ✅ Stored inline (no separate table)
- ✅ Simpler database schema

**Warning Signs:**
- Value objects inheriting from `BaseEntity`
- Value objects with `Id` property
- Separate database tables for value objects
- Equality comparison using ID

---

## Architecture Anti-Patterns

### Anti-Pattern 14: Domain Depends on Infrastructure

**What It Is:**

Domain layer referencing infrastructure concerns (database, APIs, etc.).

**Example of Anti-Pattern:**
```csharp
// ❌ DOMAIN DEPENDS ON INFRASTRUCTURE
namespace Vendo.Catalog.Domain.Entities
{
    using Microsoft.EntityFrameworkCore;  // ❌ EF Core in domain!
    using System.ComponentModel.DataAnnotations;  // ❌ Data annotations!

    [Table("Products")]  // ❌ Infrastructure concern!
    public class Product
    {
        [Key]  // ❌ Data annotation!
        public Guid Id { get; set; }

        [Required]  // ❌ Validation via attribute!
        [MaxLength(200)]  // ❌ Database constraint!
        public string Name { get; set; }

        // Domain depends on infrastructure - WRONG!
    }
}
```

**Why It's Bad:**

1. **Violates dependency rule** - domain should be innermost layer
2. **Hard to test** - domain needs infrastructure to run
3. **Tight coupling** - can't change infrastructure without changing domain
4. **Leaks implementation details**

**How We Avoided It:**

```csharp
// ✅ DOMAIN IS PURE
namespace Vendo.CatalogManagement.Domain.Entities
{
    // NO infrastructure references!
    // NO EF Core
    // NO data annotations
    // NO database concerns

    public class Product : BaseEntity
    {
        private string _name;

        public string Name => _name;

        // Validation in domain logic, not attributes
        private static void ValidateName(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (name.Length > 200)
                throw new ArgumentException("Product name cannot exceed 200 characters");
        }

        public static Product Create(...)
        {
            ValidateName(name);  // Domain validation!
            // ...
        }
    }
}

// ✅ INFRASTRUCTURE CONFIGURES DOMAIN
namespace Vendo.CatalogManagement.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;  // ✅ EF Core in infrastructure!

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Infrastructure concern - configuration here!
            builder.ToTable("Products");

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            // Domain doesn't know about any of this!
        }
    }
}
```

**Dependency Direction:**
```
Domain       (innermost - no dependencies)
   ↑
Application  (depends on Domain)
   ↑
Infrastructure (depends on Application & Domain)
   ↑
API/Web      (depends on all)
```

**Benefits:**
- ✅ Domain is pure business logic
- ✅ Easy to test (no infrastructure needed)
- ✅ Can swap infrastructure without changing domain
- ✅ Clear separation of concerns

**Warning Signs:**
- `using Microsoft.EntityFrameworkCore` in domain
- Data annotations on domain entities
- Domain referencing infrastructure projects
- Can't test domain without database

---

### Anti-Pattern 15: Shared Kernel as Dumping Ground

**What It Is:**

A "Common" or "Shared" project that becomes a dumping ground for everything.

**Example of Anti-Pattern:**
```csharp
// ❌ EVERYTHING IN COMMON
Vendo.Common/
    BaseEntity.cs         // OK - entities
    Result.cs             // OK - result pattern
    DbContext.cs          // ❌ Infrastructure!
    EmailService.cs       // ❌ Infrastructure!
    ProductDto.cs         // ❌ Application-specific!
    OrderDto.cs           // ❌ Application-specific!
    Helpers.cs            // ❌ Vague!
    Utils.cs              // ❌ Vague!
    Extensions.cs         // ❌ Vague!
    Constants.cs          // ❌ Vague!
```

**Why It's Bad:**

1. **Coupling nightmare** - everything depends on everything
2. **No clear purpose** - what belongs here?
3. **Versioning hell** - changes affect all services
4. **Unclear ownership** - who maintains it?

**How We Avoided It:**

```csharp
// ✅ MINIMAL, WELL-DEFINED SHARED CODE
Vendo.Shared/
    Domain/
        BaseEntity.cs        // Shared by all aggregates
        ValueObject.cs       // Base for value objects
        IDomainEvent.cs      // Domain event interface

// Each bounded context has its own specific code
Vendo.CatalogManagement/
    Domain/
        Entities/Product.cs       // Catalog-specific
        ValueObjects/SKU.cs       // Catalog-specific
    Application/
        DTOs/ProductDto.cs        // Catalog-specific

Vendo.OrderManagement/
    Domain/
        Entities/Order.cs         // Order-specific
    Application/
        DTOs/OrderDto.cs          // Order-specific
```

**Rule:** Only share what MUST be shared (base classes, interfaces). Keep domain-specific code in its context.

**Benefits:**
- ✅ Clear boundaries
- ✅ Independent evolution
- ✅ Minimal coupling
- ✅ Easy to version

**Warning Signs:**
- Shared project with 100+ classes
- Classes with "Helper", "Util", "Common" in name
- Application-specific code in shared project
- Every change affects all services

---

## Warning Signs

### General Warning Signs You're Falling Into Anti-Patterns

1. **Code Smells:**
   - Public setters everywhere
   - Entities are just data bags
   - Business logic in services
   - Lots of primitives (strings, decimals) for domain concepts

2. **Testing Difficulties:**
   - Hard to write unit tests
   - Tests require database
   - Mocking everything
   - Can't test in isolation

3. **Understanding Issues:**
   - Code doesn't match business language
   - Need to explain what code does
   - Unclear where to add new features
   - Frequent "where does this belong?" questions

4. **Performance Problems:**
   - Loading one thing loads everything
   - Slow aggregate loading (>100ms)
   - Many unnecessary queries
   - Concurrency conflicts

5. **Maintenance Difficulties:**
   - Scared to change code
   - Changes ripple everywhere
   - Duplicate code/validation
   - Unclear dependencies

---

## Summary

### Key Takeaways

1. **Rich Domain Model** - Put business logic in domain entities, not services
2. **Value Objects** - Use value objects for domain concepts, not primitives
3. **Small Aggregates** - Keep aggregates focused and small
4. **Domain Events** - Use events for cross-aggregate coordination
5. **Clear Boundaries** - Respect layer dependencies (Domain → Application → Infrastructure)
6. **Pure Domain** - Domain has no infrastructure dependencies
7. **Specific Repositories** - Aggregate-specific repositories, not generic
8. **Unit of Work** - Coordinate transactions and events

### Quick Reference: Anti-Pattern Checklist

Use this checklist to review your own DDD implementation:

- [ ] Are entities rich (methods) or anemic (getters/setters)?
- [ ] Do you use value objects for domain concepts?
- [ ] Do entities have private setters and business methods?
- [ ] Do domain events communicate important business events?
- [ ] Are aggregates small and focused (<1000 lines)?
- [ ] Do aggregates reference others by ID only?
- [ ] Is business logic in domain, not handlers?
- [ ] Do you have aggregate-specific repositories?
- [ ] Does Unit of Work coordinate saves and events?
- [ ] Is domain layer pure (no infrastructure dependencies)?

### Further Learning

- Read `/docs/DDD-PATTERNS-CATALOG.md` for correct patterns
- Read `/docs/AGGREGATE-DESIGN-DECISIONS.md` for boundary decisions
- Study `Product.cs` and `Category.cs` for rich domain models
- Review command handlers for proper orchestration

---

*This document is part of the Catalog Service educational documentation. Last updated: 2025-11-01*
