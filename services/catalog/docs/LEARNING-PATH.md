# Learning Path: Domain-Driven Design with Catalog Service

## Table of Contents
- [Introduction](#introduction)
- [Learning Objectives](#learning-objectives)
- [Prerequisites](#prerequisites)
- [Learning Path Overview](#learning-path-overview)
- [Phase 1: Foundations](#phase-1-foundations)
- [Phase 2: Tactical Patterns](#phase-2-tactical-patterns)
- [Phase 3: Architecture](#phase-3-architecture)
- [Phase 4: Advanced Concepts](#phase-4-advanced-concepts)
- [Practice Exercises](#practice-exercises)
- [Assessment Questions](#assessment-questions)
- [Further Resources](#further-resources)

---

## Introduction

Welcome to the **DDD Learning Path** for the Vendo Catalog Service!

This guide provides a **structured approach** to learning Domain-Driven Design using our catalog service as a real-world example. By following this path, you'll learn:

- Core DDD concepts and patterns
- How to design rich domain models
- How to structure applications using DDD
- Best practices and anti-patterns to avoid

**Time Investment:** 8-12 hours over 1-2 weeks

**Approach:** Read → Understand → Explore Code → Practice

---

## Learning Objectives

By the end of this learning path, you will be able to:

1. ✅ Explain the difference between anemic and rich domain models
2. ✅ Identify and create value objects for domain concepts
3. ✅ Design aggregates with proper boundaries
4. ✅ Use domain events for cross-aggregate communication
5. ✅ Apply the repository pattern correctly
6. ✅ Implement CQRS pattern for commands and queries
7. ✅ Structure applications using layered architecture
8. ✅ Write testable domain logic
9. ✅ Recognize and avoid common DDD anti-patterns
10. ✅ Apply ubiquitous language in code

---

## Prerequisites

**Required Knowledge:**

- ✅ C# programming (classes, interfaces, inheritance)
- ✅ Object-oriented programming basics (encapsulation, polymorphism)
- ✅ Basic understanding of databases and SQL
- ✅ Familiarity with .NET and ASP.NET Core

**Helpful (But Not Required):**

- Experience with Entity Framework Core
- Understanding of design patterns (Factory, Repository, etc.)
- Knowledge of SOLID principles
- Previous exposure to clean architecture concepts

---

## Learning Path Overview

```
Phase 1: Foundations (2-3 hours)
    ↓
Phase 2: Tactical Patterns (3-4 hours)
    ↓
Phase 3: Architecture (2-3 hours)
    ↓
Phase 4: Advanced Concepts (1-2 hours)
```

Each phase includes:
1. **Read** - Documentation and concepts
2. **Explore** - Real code examples
3. **Practice** - Hands-on exercises
4. **Verify** - Assessment questions

---

## Phase 1: Foundations

**Goal:** Understand core DDD concepts and why they matter

**Time:** 2-3 hours

---

### Step 1.1: Understand What DDD Solves

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Introduction section
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Pattern 1 (Anemic Domain Model)

**Key Concepts:**
- What is Domain-Driven Design?
- Anemic vs. Rich domain models
- Why business logic belongs in the domain

**Explore Code:**

Compare these two approaches:

**Anemic (Before DDD):**
```csharp
// ❌ Just data
public class Product
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Business logic in service
public class ProductService
{
    public void UpdateStock(Product p, int qty)
    {
        if (qty < 0) throw new Exception("Negative!");
        p.Stock = qty;
    }
}
```

**Rich (After DDD):**
```csharp
// ✅ Behavior + data
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

**Read Our Code:**
- `/src/Domain/Entities/Product.cs` - Lines 1-100 (overview)
- `/src/Domain/Common/BaseEntity.cs` - Full file

**Questions to Answer:**
1. Why can't you do `product.Price = -100` in our domain model?
2. What happens if you try to create a Product directly with `new Product()`?
3. Where is the validation logic for product name?

---

### Step 1.2: Learn Ubiquitous Language

**Read:**
- `/docs/UBIQUITOUS-LANGUAGE.md` - Full document

**Key Concepts:**
- What is ubiquitous language?
- How language shapes the model
- Terms vs. code mapping

**Explore Code:**

Find where these business terms appear in code:

| Business Term | Expected Code Element |
|--------------|----------------------|
| "Publish a product" | Method: `product.Publish()` |
| "SKU" | Value Object: `SKU` class |
| "Compare-At Price" | Property: `CompareAtPrice` |
| "Out of Stock" | Method: `product.IsOutOfStock()` |

**Read Our Code:**
- `/src/Domain/Entities/Product.cs` - Look for method names
- `/src/Domain/ValueObjects/SKU.cs` - See how business term becomes code

**Exercise:**

Open `Product.cs` and find:
1. How many business methods can you identify? (Hint: Count public methods)
2. Which method would you call to change a product's price?
3. What business rule prevents publishing a product? (Hint: Check `Publish()` method)

**Self-Check:**
- Can you explain what "Compare-At Price" means to a developer?
- Can you explain what `product.SetCompareAtPrice()` does to a business person?
- Do they match?

---

### Step 1.3: Understand Entity vs. Value Object

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Sections 1 (Entities) and 2 (Value Objects)
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Patterns 2 (Primitive Obsession) and 12 (Mutable Value Objects)

**Key Concepts:**
- Entities have identity (compared by ID)
- Value objects have no identity (compared by value)
- Value objects are immutable
- When to use each

**Compare:**

| Aspect | Entity (Product) | Value Object (Money) |
|--------|-----------------|---------------------|
| **Identity** | Has unique ID | No identity |
| **Equality** | By ID | By value |
| **Mutability** | Mutable (via methods) | Immutable |
| **Lifecycle** | Independent | Owned by entity |
| **Example** | Two products with ID=1 are the same product | Two Money(100, USD) are identical |

**Explore Code:**

**Entity:**
```csharp
// src/Domain/Entities/Product.cs
public class Product : BaseEntity
{
    public Guid Id { get; private set; }  // Has identity!

    // Can change over time (mutable)
    public void ChangePrice(Money newPrice, string updatedBy) { }
}
```

**Value Object:**
```csharp
// src/Domain/ValueObjects/Money.cs
public sealed class Money : ValueObject
{
    // No Id property! No identity!

    public decimal Amount { get; }  // Immutable!
    public string Currency { get; }

    // To "change", create new instance
    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount, Currency);  // New object!
    }
}
```

**Read Our Code:**
- `/src/Domain/Entities/Product.cs` - Full entity
- `/src/Domain/ValueObjects/Money.cs` - Full value object
- `/src/Domain/ValueObjects/SKU.cs` - Another value object

**Exercise:**

For each of these, decide if it should be an Entity or Value Object:

1. Customer Order - ?
2. Shipping Address - ?
3. Email Address - ?
4. Product Category - ?
5. Product Image URL - ?

<details>
<summary>Click for answers</summary>

1. **Entity** - Has identity (order number), tracked over time
2. **Value Object** - No identity, compared by value (same street/city/zip = same address)
3. **Value Object** - No identity, "john@example.com" is just a value
4. **Entity** - Has identity, categories are tracked and modified over time
5. **Value Object** - Just a URL string, no identity needed
</details>

---

## Phase 2: Tactical Patterns

**Goal:** Master the tactical DDD patterns we use

**Time:** 3-4 hours

---

### Step 2.1: Value Objects Deep Dive

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Section 2 (Value Objects) - All examples
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Patterns 12 & 13

**Key Concepts:**
- Creating value objects with factory methods
- Validation at creation
- Immutability
- Value equality
- Operations return new instances

**Explore All Our Value Objects:**

| Value Object | File | Purpose |
|-------------|------|---------|
| `Money` | `/src/Domain/ValueObjects/Money.cs` | Monetary values |
| `SKU` | `/src/Domain/ValueObjects/SKU.cs` | Product identifiers |
| `Slug` | `/src/Domain/ValueObjects/Slug.cs` | URL-friendly names |
| `SEOMetadata` | `/src/Domain/ValueObjects/SEOMetadata.cs` | Search metadata |
| `ProductImages` | `/src/Domain/ValueObjects/ProductImages.cs` | Image collections |
| `Dimensions` | `/src/Domain/ValueObjects/Dimensions.cs` | Physical dimensions |

**Read Code:**
- `/src/Domain/ValueObjects/Money.cs` - Complete file
- `/src/Domain/ValueObjects/SKU.cs` - Complete file

**Pattern to Learn:**

```csharp
public sealed class SKU : ValueObject
{
    // 1. PRIVATE constructor
    private SKU(string value)
    {
        Value = value;
    }

    // 2. PUBLIC factory method with validation
    public static SKU? Create(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (!SkuPattern.IsMatch(normalized)) return null;  // Validation!

        return new SKU(normalized);
    }

    // 3. IMMUTABLE properties
    public string Value { get; }

    // 4. VALUE equality
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Exercise:**

Create a new value object for "Email Address":

Requirements:
- Private constructor
- Factory method `Create(string email)` that validates format
- Returns `null` if invalid
- Immutable `Value` property
- Normalizes to lowercase
- Must contain @ symbol
- Must have domain part (e.g., "example.com")

<details>
<summary>Sample solution</summary>

```csharp
public sealed class Email : ValueObject
{
    private static readonly Regex EmailPattern =
        new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailPattern.IsMatch(normalized)) return null;

        return new Email(normalized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```
</details>

**Self-Check:**
- Can you create an invalid `SKU`? (Answer: No - factory returns null)
- Can you modify a `Money` object after creation? (Answer: No - immutable)
- Are two `Money(100, USD)` instances equal? (Answer: Yes - value equality)

---

### Step 2.2: Aggregates and Boundaries

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Section 3 (Aggregates)
- `/docs/AGGREGATE-DESIGN-DECISIONS.md` - Full document
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Patterns 5, 6, 7

**Key Concepts:**
- What is an aggregate?
- Aggregate root
- Consistency boundaries
- Transaction boundaries
- How to identify aggregate boundaries

**Our Aggregates:**

1. **Product Aggregate**
   - Root: `Product` entity
   - Includes: SKU, Money, ProductImages, SEOMetadata, Dimensions, Tags
   - Boundary: Everything that must be consistent with product

2. **Category Aggregate**
   - Root: `Category` entity
   - Includes: Slug, SEOMetadata
   - Boundary: Everything that must be consistent with category

**Explore Code:**

Read and understand:
- `/src/Domain/Entities/Product.cs` - Lines 1-50 (fields and properties)
- `/src/Domain/Entities/Category.cs` - Complete file

**Key Pattern:**

```csharp
public class Product : BaseEntity  // Aggregate Root
{
    // INCLUDED in aggregate (value objects)
    private Money _price;              // ✅ Part of aggregate
    private SKU _sku;                  // ✅ Part of aggregate
    private ProductImages? _images;    // ✅ Part of aggregate

    // REFERENCE to other aggregate (ID only, not object)
    public Guid CategoryId { get; private set; }  // ✅ Reference by ID

    // NOT INCLUDED (no navigation property)
    // public Category Category { get; set; }  ❌ Would violate boundary!
}
```

**Exercise:**

Answer these aggregate design questions:

1. **Should `ProductVariant` be in the Product aggregate or separate?**
   - Consider: < 10 variants per product
   - Consider: Variants share product's business rules
   - Consider: Changing variant affects product total stock

2. **Should `ProductReview` be in the Product aggregate?**
   - Consider: Thousands of reviews per product
   - Consider: Adding review doesn't change product
   - Consider: Reviews and products have different lifecycles

<details>
<summary>Answers with reasoning</summary>

1. **ProductVariant: Inside Product aggregate (if < 50 variants)**
   - Reason: Small number, shared business rules, need consistency
   - INVARIANT: "All variants must have unique SKU"
   - This invariant requires variants to be in same aggregate as product

2. **ProductReview: Separate aggregate**
   - Reason: Large number (thousands), independent lifecycle
   - Adding review doesn't need to lock product
   - Reviews and products can be eventually consistent
   - Better scalability
</details>

**Read Decision Examples:**
- `/docs/AGGREGATE-DESIGN-DECISIONS.md` - Decision 1 (Product vs Category)
- `/docs/AGGREGATE-DESIGN-DECISIONS.md` - Decision 2 (ProductImages)

---

### Step 2.3: Domain Events

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Section 4 (Domain Events)
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Pattern 4 (Missing Events)

**Key Concepts:**
- What are domain events?
- When to raise events
- How events enable decoupling
- Event handlers vs. direct calls

**Our Domain Events:**

| Event | When Raised | Business Significance |
|-------|------------|----------------------|
| `ProductCreatedEvent` | New product created | Initialize search index, analytics |
| `ProductPriceChangedEvent` | Price changes | Update price history, notify subscribers |
| `ProductStockChangedEvent` | Stock changes | Alert if low stock, update availability |
| `ProductPublishedEvent` | Product published | Index in search, trigger marketing |
| `CategoryCreatedEvent` | New category created | Update navigation, flush cache |

**Explore Code:**

Read these files in order:

1. `/src/Domain/Events/IDomainEvent.cs` - Interface
2. `/src/Domain/Events/ProductPriceChangedEvent.cs` - Example event
3. `/src/Domain/Common/BaseEntity.cs` - How events are collected
4. `/src/Infrastructure/Persistence/UnitOfWork.cs` - How events are dispatched

**Pattern:**

```csharp
// 1. ENTITY raises event
public class Product : BaseEntity
{
    public void ChangePrice(Money newPrice, string updatedBy)
    {
        _price = newPrice;
        UpdateAudit(updatedBy);

        // Raise event - don't handle side effects here!
        AddDomainEvent(new ProductPriceChangedEvent(Id, _price.Amount));
    }
}

// 2. HANDLER reacts to event (decoupled)
public class ProductPriceChangedHandler : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent evt, CancellationToken ct)
    {
        // Side effect - update price history
        await _priceHistoryService.RecordPriceChange(evt.ProductId, evt.NewPrice);
    }
}

// 3. UNIT OF WORK dispatches automatically
public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    await DispatchDomainEventsAsync(ct);  // Automatic!
    return await _context.SaveChangesAsync(ct);
}
```

**Exercise:**

Design a new domain event: `ProductDiscontinuedEvent`

Requirements:
1. What properties should it have?
2. When should it be raised?
3. What handlers might react to it? (Think of side effects)

<details>
<summary>Sample solution</summary>

```csharp
// Event
public record ProductDiscontinuedEvent(
    Guid ProductId,
    string ProductName,
    Guid CategoryId,
    DateTime DiscontinuedAt
) : IDomainEvent;

// Raised when:
public void Discontinue(string updatedBy)
{
    Status = ProductStatus.Discontinued;
    UpdateAudit(updatedBy);
    AddDomainEvent(new ProductDiscontinuedEvent(Id, Name, CategoryId, DateTime.UtcNow));
}

// Potential handlers:
// 1. UpdateSearchIndexHandler - Remove from search
// 2. NotifySubscribersHandler - Email customers on waitlist
// 3. UpdateAnalyticsHandler - Record discontinuation
// 4. ClearCacheHandler - Invalidate cache entries
```
</details>

---

### Step 2.4: Repositories and Unit of Work

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Section 5 (Repositories)
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Patterns 10 & 11

**Key Concepts:**
- Repository pattern
- Aggregate-specific repositories
- Unit of Work pattern
- Transaction coordination

**Explore Code:**

Read in this order:

1. `/src/Domain/Interfaces/IProductRepository.cs` - Repository interface
2. `/src/Domain/Interfaces/IUnitOfWork.cs` - Unit of Work interface
3. `/src/Infrastructure/Persistence/Repositories/ProductRepository.cs` - Implementation
4. `/src/Infrastructure/Persistence/UnitOfWork.cs` - Unit of Work implementation

**Pattern:**

```csharp
// REPOSITORY INTERFACE (in Domain)
public interface IProductRepository
{
    // Domain language methods
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<List<Product>> GetLowStockProductsAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    void Remove(Product product);
    // NO SaveChangesAsync! That's Unit of Work's job
}

// UNIT OF WORK (coordinates repositories)
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// USAGE in Application Layer
public class CreateProductCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result> Handle(CreateProductCommand request)
    {
        var product = Product.Create(...);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();  // Single save!

        return Result.Success(product.Id);
    }
}
```

**Compare Anti-Pattern:**

```csharp
// ❌ WRONG: Generic repository
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    // Exposes implementation details!
}

// ❌ WRONG: Repository with SaveChanges
public interface IProductRepository
{
    Task SaveChangesAsync();  // Each repo has its own save!
}

// ✅ CORRECT: Aggregate-specific, Unit of Work coordinates
public interface IProductRepository
{
    Task<Product?> GetBySkuAsync(string sku);  // Domain language!
    // NO Save method
}
```

**Self-Check:**
- Why don't repositories have `SaveChangesAsync()`? (Answer: Unit of Work coordinates)
- Why is `GetBySkuAsync()` better than `FindAsync(p => p.SKU == sku)`? (Answer: Domain language, encapsulates query)
- Can you update multiple aggregates in one transaction? (Answer: Via Unit of Work, but should be rare)

---

## Phase 3: Architecture

**Goal:** Understand how DDD layers work together

**Time:** 2-3 hours

---

### Step 3.1: Layered Architecture

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - Section "Architectural Patterns"
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - Anti-Pattern 14 (Domain Depends on Infrastructure)

**Key Concepts:**
- Four layers: Domain, Application, Infrastructure, API
- Dependency direction (inward)
- Separation of concerns
- Dependency Inversion Principle

**Our Layers:**

```
┌─────────────────────────────────────────┐
│   API Layer (WebAPI)                     │  ← Presentation
│   - Controllers                          │
│   - Middleware                           │
│   - Filters                              │
└─────────────────────────────────────────┘
            ↓ depends on
┌─────────────────────────────────────────┐
│   Infrastructure Layer                   │  ← External concerns
│   - EF Core DbContext                    │
│   - Repository implementations           │
│   - External services                    │
└─────────────────────────────────────────┘
            ↓ depends on
┌─────────────────────────────────────────┐
│   Application Layer                      │  ← Use cases
│   - Command handlers                     │
│   - Query handlers                       │
│   - DTOs                                 │
│   - Validators                           │
└─────────────────────────────────────────┘
            ↓ depends on
┌─────────────────────────────────────────┐
│   Domain Layer                           │  ← Business logic
│   - Entities                             │  ← No dependencies!
│   - Value Objects                        │
│   - Domain Events                        │
│   - Repository Interfaces                │
└─────────────────────────────────────────┘
```

**Dependency Rule:** Inner layers don't know about outer layers

**Explore Project Structure:**

```
/services/catalog/
├── src/
│   ├── Domain/                    ← Core business logic
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Events/
│   │   └── Interfaces/
│   ├── Application/               ← Use cases
│   │   ├── Products/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   └── Categories/
│   ├── Infrastructure/            ← External concerns
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   └── Services/
│   └── Api/                       ← HTTP API
│       └── Controllers/
└── tests/
    └── Domain.Tests/              ← Unit tests
```

**Exercise:**

For each of these, decide which layer it belongs in:

1. `ProductController.cs` with `[ApiController]` attribute - ?
2. `Product.cs` entity with business methods - ?
3. `CreateProductCommandHandler.cs` - ?
4. `CatalogDbContext.cs` with EF Core mappings - ?
5. `IProductRepository.cs` interface - ?
6. `ProductRepository.cs` implementation using EF Core - ?

<details>
<summary>Answers</summary>

1. **API Layer** - HTTP concerns
2. **Domain Layer** - Core business logic
3. **Application Layer** - Use case orchestration
4. **Infrastructure Layer** - Database implementation
5. **Domain Layer** - Interface (Domain defines what it needs)
6. **Infrastructure Layer** - Implementation (Infrastructure provides it)
</details>

**Read Code:**

Compare these files to see layer separation:

- `/src/Domain/Entities/Product.cs` - Pure domain (no EF Core!)
- `/src/Infrastructure/Persistence/Configurations/ProductConfiguration.cs` - EF Core mapping
- `/src/Application/Products/Commands/CreateProduct/CreateProductCommandHandler.cs` - Orchestration

---

### Step 3.2: CQRS Pattern

**Read:**
- `/docs/DDD-PATTERNS-CATALOG.md` - CQRS section

**Key Concepts:**
- Command Query Responsibility Segregation
- Commands: Write operations, change state
- Queries: Read operations, return data
- Different models for read and write

**Our Implementation:**

**Commands (Write Side):**
- `/src/Application/Products/Commands/CreateProduct/`
  - `CreateProductCommand.cs` - Request
  - `CreateProductCommandHandler.cs` - Handler
  - `CreateProductCommandValidator.cs` - Validation

**Queries (Read Side):**
- `/src/Application/Products/Queries/GetProductById/`
  - `GetProductByIdQuery.cs` - Request
  - `GetProductByIdQueryHandler.cs` - Handler

**Pattern:**

```csharp
// COMMAND (writes)
public record CreateProductCommand(
    string Name,
    string Description,
    string SKU,
    decimal Price
) : IRequest<Result<Guid>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Create domain entities
        var product = Product.Create(...);

        // 2. Persist
        await _unitOfWork.Products.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 3. Return ID
        return Result.Success(product.Id);
    }
}

// QUERY (reads)
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        // Direct database query, bypass domain
        return await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price.Amount,
                // Map to DTO
            })
            .FirstOrDefaultAsync(ct);
    }
}
```

**Key Differences:**

| Aspect | Command | Query |
|--------|---------|-------|
| **Purpose** | Change state | Read data |
| **Returns** | Result/void | DTO |
| **Uses** | Domain entities | Direct DB queries |
| **Validation** | FluentValidation | Minimal |
| **Events** | Raises events | No events |

**Explore Code:**

Read complete command flow:
1. `/src/Application/Products/Commands/CreateProduct/CreateProductCommand.cs`
2. `/src/Application/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`
3. `/src/Application/Products/Commands/CreateProduct/CreateProductCommandValidator.cs`

Read complete query flow:
1. `/src/Application/Products/Queries/GetProductById/GetProductByIdQuery.cs`
2. `/src/Application/Products/Queries/GetProductById/GetProductByIdQueryHandler.cs`

**Self-Check:**
- Should queries use `IUnitOfWork.SaveChangesAsync()`? (Answer: No - queries don't modify)
- Can commands return full DTOs? (Answer: Usually just ID/Result, not full DTOs)
- Should commands raise domain events? (Answer: Yes, indirectly via domain methods)

---

### Step 3.3: Dependency Injection and Registration

**Read:**
- `/src/Infrastructure/DependencyInjection.cs`
- `/src/Application/DependencyInjection.cs` (if exists)

**Key Concepts:**
- Service registration
- Lifetimes (Transient, Scoped, Singleton)
- How layers are wired together

**Pattern:**

```csharp
// Infrastructure registers implementations
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories (registered via Unit of Work, not directly)

        return services;
    }
}

// Application registers handlers
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

services.AddValidatorsFromAssembly(typeof(CreateProductCommandValidator).Assembly);
```

**Explore Code:**
- `/src/Infrastructure/DependencyInjection.cs` - Full file

---

## Phase 4: Advanced Concepts

**Goal:** Master advanced DDD techniques

**Time:** 1-2 hours

---

### Step 4.1: Testing Domain Logic

**Read:**
- `/tests/Domain.Tests/Entities/ProductTests.cs`
- `/tests/Domain.Tests/ValueObjects/MoneyTests.cs`
- `/tests/Domain.Tests/ValueObjects/SKUTests.cs`

**Key Concepts:**
- Unit testing domain logic
- Arrange-Act-Assert pattern
- Testing invariants
- Testing domain events

**Pattern:**

```csharp
[Fact]
public void ChangePrice_ShouldUpdatePriceAndRaiseEvent()
{
    // Arrange - Create test data
    var product = CreateTestProduct();  // Helper method
    var newPrice = Money.Create(149.99m)!;

    // Act - Execute domain method
    product.ChangePrice(newPrice, "admin");

    // Assert - Verify results
    product.Price.Amount.Should().Be(149.99m);
    product.DomainEvents.Should().Contain(e => e is ProductPriceChangedEvent);
}

[Fact]
public void SetCompareAtPrice_LessThanPrice_ShouldThrowException()
{
    // Arrange
    var product = CreateTestProduct();  // Price: 99.99
    var compareAtPrice = Money.Create(49.99m)!;  // Less than price!

    // Act
    var act = () => product.SetCompareAtPrice(compareAtPrice, "admin");

    // Assert - Should throw
    act.Should().Throw<InvalidOperationException>()
        .WithMessage("*greater than current price*");
}
```

**Exercise:**

Write a test for this scenario:

**Requirement:** "Cannot publish a product without images"

<details>
<summary>Sample test</summary>

```csharp
[Fact]
public void Publish_WithoutImages_ShouldThrowException()
{
    // Arrange
    var product = Product.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Test Product",
        "Description",
        SKU.Create("TEST-001")!,
        Money.Create(99.99m)!,
        null,  // No images!
        "admin"
    );

    // Act
    var act = () => product.Publish("admin");

    // Assert
    act.Should().Throw<InvalidOperationException>()
        .WithMessage("*without images*");
}
```
</details>

**Run Tests:**

```bash
cd /services/catalog/tests/Domain.Tests
dotnet test
```

**Self-Check:**
- Can you test domain logic without a database? (Answer: Yes!)
- Do domain tests need EF Core? (Answer: No - pure unit tests)
- Should you test private methods? (Answer: No - test public API)

---

### Step 4.2: Evolution and Refactoring

**Read:**
- `/docs/UBIQUITOUS-LANGUAGE.md` - "Evolution of Language" section

**Key Concepts:**
- Domain models evolve
- Language discoveries lead to refactorings
- Adding new concepts
- Backward compatibility

**Example Evolution:**

**Version 1: Simple Price**
```csharp
public class Product
{
    public decimal Price { get; set; }  // Just a number
}
```

**Version 2: Price with Currency**
```csharp
public class Product
{
    public decimal Price { get; set; }
    public string Currency { get; set; }
}
```

**Version 3: Money Value Object**
```csharp
public class Product
{
    private Money _price;
    public Money Price => _price;
}
```

**Version 4: Compare-At Price Added**
```csharp
public class Product
{
    private Money _price;
    private Money? _compareAtPrice;

    public void SetCompareAtPrice(Money compareAtPrice, string updatedBy)
    {
        if (!compareAtPrice.IsGreaterThan(_price))
            throw new InvalidOperationException("...");

        _compareAtPrice = compareAtPrice;
    }
}
```

**Exercise:**

Your business wants to add "Product Variants" (e.g., T-shirt in Small/Medium/Large).

Design the aggregate boundary:
1. Should variants be in the Product aggregate or separate?
2. If in Product aggregate, how would you model them?
3. What invariants would you enforce?
4. What domain events would you raise?

<details>
<summary>Design considerations</summary>

**Option A: Variants inside Product (if < 50 variants)**

```csharp
public class Product : BaseEntity
{
    private List<ProductVariant> _variants = new();

    public void AddVariant(string size, string color, SKU sku, int stock, string createdBy)
    {
        // INVARIANT: Each variant must have unique SKU
        if (_variants.Any(v => v.SKU == sku))
            throw new InvalidOperationException("Duplicate variant SKU");

        var variant = ProductVariant.Create(Id, size, color, sku, stock);
        _variants.Add(variant);

        AddDomainEvent(new ProductVariantAddedEvent(Id, variant.Id));
    }

    public int GetTotalStock() => _variants.Sum(v => v.Stock);
}

// Child entity (not separate aggregate)
public class ProductVariant
{
    public Guid ProductId { get; private set; }
    public string Size { get; private set; }
    public string Color { get; private set; }
    public SKU SKU { get; private set; }
    public int Stock { get; private set; }
}
```

**Invariants:**
- All variant SKUs must be unique
- Total stock = sum of variant stocks
- Can't delete product if variants exist

**Events:**
- `ProductVariantAddedEvent`
- `ProductVariantStockChangedEvent`
- `ProductVariantRemovedEvent`

**Option B: Variants as separate aggregate (if > 50 variants)**
- Better for large catalogs with many variants
- Eventual consistency between product and variants
- Better scalability
</details>

---

## Practice Exercises

### Exercise 1: Add "Wishlist Count" Feature

**Requirement:** Track how many wishlists each product appears in.

**Tasks:**
1. Add `WishlistCount` property to Product
2. Add method `IncrementWishlistCount()` with business rules:
   - Can't increment if product is Discontinued
   - Can't be negative
3. Raise domain event `ProductAddedToWishlistEvent`
4. Write unit tests

**Hints:**
- Look at how `StockQuantity` is implemented
- Follow the same pattern: private setter, business method, event

---

### Exercise 2: Implement "Discount Code" Value Object

**Requirement:** Create a value object for discount codes (e.g., "SUMMER25").

**Rules:**
- 5-20 characters
- Uppercase letters and numbers only
- Must start with letter
- Optional: Can specify expiration date

**Tasks:**
1. Create `DiscountCode` value object
2. Factory method with validation
3. Method `IsExpired()` if expiration date set
4. Write unit tests

---

### Exercise 3: Design "Product Bundle" Aggregate

**Requirement:** Create product bundles (e.g., "Gaming Starter Pack" containing keyboard, mouse, headset).

**Tasks:**
1. Decide: Separate aggregate or inside Product?
2. Design the entity/value object structure
3. Identify invariants (e.g., "Bundle price must be less than sum of individual prices")
4. Identify domain events
5. Write pseudocode for main methods

**Consider:**
- How many products in a bundle? (Affects aggregate size)
- Do bundles have their own inventory?
- Can a bundle contain other bundles?

---

## Assessment Questions

Test your understanding with these questions:

### Beginner Level

1. **What's the difference between an entity and a value object?**
   <details><summary>Answer</summary>
   Entity has identity (ID), value object has no identity and is compared by value. Entities are mutable (via methods), value objects are immutable.
   </details>

2. **Why can't you do `product.Price = -100`?**
   <details><summary>Answer</summary>
   Price is a private backing field with no public setter. Must use `ChangePrice()` method which validates. This encapsulation prevents invalid state.
   </details>

3. **What is ubiquitous language?**
   <details><summary>Answer</summary>
   Shared vocabulary used by both developers and domain experts. Same terms in conversation and code (e.g., business says "Publish", code has `Publish()` method).
   </details>

### Intermediate Level

4. **Why are Product and Category separate aggregates?**
   <details><summary>Answer</summary>
   Different lifecycles, different transaction boundaries, scalability (category can have 1000s of products - loading all would be slow), independent modification.
   </details>

5. **When should you raise a domain event?**
   <details><summary>Answer</summary>
   When something significant happens in the business domain (price changed, product published, stock updated). Events enable decoupled reactions without tight coupling.
   </details>

6. **Why don't repositories have `SaveChangesAsync()`?**
   <details><summary>Answer</summary>
   Unit of Work coordinates saves across multiple repositories in a single transaction. It also dispatches domain events atomically before saving.
   </details>

### Advanced Level

7. **How would you implement cross-aggregate transactions?**
   <details><summary>Answer</summary>
   Avoid if possible (one aggregate per transaction). If must, use domain events - first aggregate saves, raises event, handler updates second aggregate in separate transaction (eventual consistency).
   </details>

8. **When should you split an aggregate into smaller aggregates?**
   <details><summary>Answer</summary>
   When: loading is slow (>100ms), many unrelated business methods, frequent concurrent modification conflicts, entities rarely change together, collection of child entities has 100s+ items.
   </details>

9. **How do you handle aggregates that reference each other?**
   <details><summary>Answer</summary>
   Reference by ID only (not object reference). Load separately through repositories. Use domain events for coordination. Use query models (CQRS) for displays that need data from multiple aggregates.
   </details>

---

## Further Resources

### Official DDD Resources

- **Book:** "Domain-Driven Design" by Eric Evans (The Blue Book)
- **Book:** "Implementing Domain-Driven Design" by Vaughn Vernon (The Red Book)
- **Book:** "Domain-Driven Design Distilled" by Vaughn Vernon (Quick introduction)

### Online Learning

- **DDD Community:** https://github.com/ddd-crew
- **Patterns:** https://www.dddcommunity.org/patterns/
- **Examples:** https://github.com/dotnet-architecture/eShopOnContainers

### Our Documentation

You've read these, but they're great references:

- `/docs/DDD-PATTERNS-CATALOG.md` - All patterns with examples
- `/docs/AGGREGATE-DESIGN-DECISIONS.md` - Boundary decisions explained
- `/docs/UBIQUITOUS-LANGUAGE.md` - Complete term dictionary
- `/docs/DDD-ANTI-PATTERNS-AVOIDED.md` - What NOT to do

### Practice Projects

**Level 1: Small Additions**
- Add new value object (Email, PhoneNumber, Address)
- Add new business method to existing entity
- Add new domain event

**Level 2: New Aggregate**
- Create "ProductReview" aggregate
- Create "InventoryAdjustment" aggregate
- Create "PriceHistory" aggregate

**Level 3: New Bounded Context**
- Design "Shopping Cart" context (separate from Catalog)
- Design "Wishlist" context
- Design "Recommendation" context

---

## Completion Checklist

Mark off as you complete each phase:

- [ ] Phase 1.1: Understand Anemic vs Rich domain models
- [ ] Phase 1.2: Learn ubiquitous language
- [ ] Phase 1.3: Understand Entity vs Value Object
- [ ] Phase 2.1: Master value objects
- [ ] Phase 2.2: Understand aggregates and boundaries
- [ ] Phase 2.3: Use domain events
- [ ] Phase 2.4: Apply repositories and Unit of Work
- [ ] Phase 3.1: Understand layered architecture
- [ ] Phase 3.2: Implement CQRS pattern
- [ ] Phase 3.3: Configure dependency injection
- [ ] Phase 4.1: Write domain unit tests
- [ ] Phase 4.2: Handle evolution and refactoring
- [ ] Complete Practice Exercise 1
- [ ] Complete Practice Exercise 2
- [ ] Complete Practice Exercise 3
- [ ] Answer all assessment questions

---

## Next Steps

After completing this learning path:

1. **Apply to Real Projects**
   - Use DDD in your own projects
   - Start with small aggregates
   - Grow your domain model iteratively

2. **Join DDD Community**
   - Participate in DDD discussions
   - Share your learnings
   - Learn from others' experiences

3. **Deep Dive Topics**
   - Event Sourcing
   - CQRS with separate read/write databases
   - Domain-Driven Design in microservices
   - Bounded contexts and context mapping

4. **Read More Advanced Material**
   - Vernon's "Implementing Domain-Driven Design"
   - Evans' "Domain-Driven Design Reference"
   - "Patterns, Principles, and Practices of Domain-Driven Design" by Millett & Tune

---

## Feedback

This learning path is designed to be practical and hands-on. As you work through it:

- Make notes of concepts that clicked
- Mark sections that need clarification
- Try the exercises and compare with provided solutions
- Build something new using these patterns

**Remember:** DDD is learned by doing. Reading is the first step - applying is where real learning happens.

---

*This learning path is part of the Catalog Service educational documentation. Last updated: 2025-11-01*
