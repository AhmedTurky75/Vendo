// ============================================================================
// EXAMPLE: Complete Flow - Creating a Product (End-to-End)
// ============================================================================
//
// This example shows the COMPLETE flow of creating a product, from HTTP request
// all the way to database persistence, demonstrating how all DDD layers work
// together.
//
// FLOW: API → Application → Domain → Infrastructure → Database
//
// KEY LEARNING:
// - How layers interact
// - Dependency direction (always inward)
// - Separation of concerns
// - Role of each layer
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Vendo.CatalogManagement.Examples.CompleteFlow
{
    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                    LAYER 1: API / PRESENTATION                         ║
    // ║                                                                        ║
    // ║  Responsibilities:                                                     ║
    // ║  - Receive HTTP requests                                               ║
    // ║  - Validate request format (not business rules)                        ║
    // ║  - Map HTTP request to command                                         ║
    // ║  - Send command to application layer                                   ║
    // ║  - Return HTTP response                                                ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// HTTP Request DTO - What comes from the client
    /// </summary>
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public List<string> ImageUrls { get; set; }
        public Guid CategoryId { get; set; }
    }

    /// <summary>
    /// HTTP Response DTO - What we send back to client
    /// </summary>
    public class CreateProductResponse
    {
        public Guid ProductId { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// API Controller - Entry point for HTTP requests
    ///
    /// This is THIN - just HTTP concerns:
    /// - Receives HTTP request
    /// - Maps to command
    /// - Sends to application layer
    /// - Maps result to HTTP response
    /// </summary>
    public class ProductsController
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/products
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {
            // ✅ Controller is THIN - just orchestration

            // 1. Map HTTP request to command
            var command = new CreateProductCommand(
                Name: request.Name,
                Description: request.Description,
                SKU: request.SKU,
                Price: request.Price,
                Currency: request.Currency,
                ImageUrls: request.ImageUrls,
                CategoryId: request.CategoryId
            );

            // 2. Send command to application layer (via MediatR)
            var result = await _mediator.Send(command);

            // 3. Map result to HTTP response
            if (result.IsSuccess)
            {
                var response = new CreateProductResponse
                {
                    ProductId = result.Value,
                    Message = "Product created successfully"
                };

                return new CreatedResult($"/api/products/{result.Value}", response);
            }

            return new BadRequestResult(result.Error);
        }
    }

    // Simplified ASP.NET Core types for example
    public interface IActionResult { }
    public class CreatedResult : IActionResult
    {
        public CreatedResult(string location, object value) { }
    }
    public class BadRequestResult : IActionResult
    {
        public BadRequestResult(string error) { }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                   LAYER 2: APPLICATION LAYER                           ║
    // ║                                                                        ║
    // ║  Responsibilities:                                                     ║
    // ║  - Define use cases (commands/queries)                                 ║
    // ║  - Orchestrate domain operations                                       ║
    // ║  - Validate business inputs                                            ║
    // ║  - Coordinate infrastructure                                           ║
    // ║  - Map domain to DTOs                                                  ║
    // ║                                                                        ║
    // ║  Does NOT:                                                             ║
    // ║  - Contain business logic (that's in domain)                           ║
    // ║  - Know about HTTP, database, etc.                                     ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Command - Represents the intent to create a product
    /// This is a "request" object that goes through the application layer
    /// </summary>
    public record CreateProductCommand(
        string Name,
        string Description,
        string SKU,
        string Currency,
        decimal Price,
        List<string> ImageUrls,
        Guid CategoryId
    ) : IRequest<Result<Guid>>;

    /// <summary>
    /// Command Validator - Validates inputs using FluentValidation
    ///
    /// This validates FORMAT, not business rules:
    /// - Is name provided? ✅
    /// - Is price a positive number? ✅
    /// - Does category exist in DB? ❌ (that's business validation)
    /// </summary>
    public class CreateProductCommandValidator : IValidator<CreateProductCommand>
    {
        public bool Validate(CreateProductCommand command, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(command.Name))
                errors.Add("Product name is required");

            if (command.Name?.Length > 200)
                errors.Add("Product name cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(command.SKU))
                errors.Add("SKU is required");

            if (command.Price < 0)
                errors.Add("Price cannot be negative");

            if (command.CategoryId == Guid.Empty)
                errors.Add("Category ID is required");

            return !errors.Any();
        }
    }

    /// <summary>
    /// Command Handler - Orchestrates the use case
    ///
    /// This is WHERE THE MAGIC HAPPENS:
    /// 1. Validates business rules (using domain)
    /// 2. Creates value objects
    /// 3. Calls domain factory method
    /// 4. Persists via Unit of Work
    /// 5. Returns result
    ///
    /// Notice: NO BUSINESS LOGIC HERE - just orchestration!
    /// </summary>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICreateProductCommandValidator _validator;

        public CreateProductCommandHandler(
            IUnitOfWork unitOfWork,
            ICreateProductCommandValidator validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<Result<Guid>> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            // STEP 1: Validate command format
            if (!_validator.Validate(request, out var errors))
                return Result<Guid>.Failure(string.Join(", ", errors));

            // STEP 2: Business validations

            // Check if SKU is unique (business rule!)
            var existingProduct = await _unitOfWork.Products
                .GetBySkuAsync(request.SKU, cancellationToken);

            if (existingProduct != null)
                return Result<Guid>.Failure($"SKU '{request.SKU}' already exists");

            // Check if category exists (business rule!)
            var category = await _unitOfWork.Categories
                .GetByIdAsync(request.CategoryId, cancellationToken);

            if (category == null)
                return Result<Guid>.Failure("Category not found");

            // STEP 3: Create value objects (domain concepts)

            var sku = SKU.Create(request.SKU);
            if (sku == null)
                return Result<Guid>.Failure("Invalid SKU format");

            var price = Money.Create(request.Price, request.Currency);
            if (price == null)
                return Result<Guid>.Failure("Invalid price or currency");

            ProductImages? images = null;
            if (request.ImageUrls?.Any() == true)
            {
                images = ProductImages.Create(request.ImageUrls);
                if (images == null)
                    return Result<Guid>.Failure("Invalid image URLs");
            }

            // STEP 4: Call domain factory method
            // This is where domain logic executes!
            var product = Product.Create(
                tenantId: Guid.NewGuid(), // Would come from authenticated user
                categoryId: request.CategoryId,
                name: request.Name,
                description: request.Description,
                sku: sku,
                price: price,
                images: images,
                createdBy: "system" // Would come from authenticated user
            );

            // Domain event is automatically raised inside Product.Create()!

            // STEP 5: Persist via repository
            await _unitOfWork.Products.AddAsync(product, cancellationToken);

            // STEP 6: Save (this also dispatches domain events!)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // STEP 7: Return success with product ID
            return Result<Guid>.Success(product.Id);
        }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                      LAYER 3: DOMAIN LAYER                             ║
    // ║                                                                        ║
    // ║  Responsibilities:                                                     ║
    // ║  - Business logic and rules                                            ║
    // ║  - Domain entities and value objects                                   ║
    // ║  - Domain events                                                       ║
    // ║  - Invariant protection                                                ║
    // ║                                                                        ║
    // ║  Does NOT:                                                             ║
    // ║  - Know about database, HTTP, infrastructure                           ║
    // ║  - Depend on any outer layer                                           ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Domain Entity - Product Aggregate Root
    ///
    /// This contains ALL business logic related to Product:
    /// - Creation rules
    /// - Validation
    /// - Business operations
    /// - Invariant protection
    /// </summary>
    public class Product
    {
        // Private backing fields
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Guid CategoryId { get; private set; }

        private string _name;
        private string? _description;
        private SKU _sku;
        private Money _price;
        private ProductImages? _images;
        private ProductStatus _status;

        // Domain events
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents =>
            _domainEvents.AsReadOnly();

        // Properties
        public string Name => _name;
        public SKU SKU => _sku;
        public Money Price => _price;
        public ProductImages? Images => _images;
        public ProductStatus Status => _status;

        // ✅ Private constructor - can't create directly
        private Product() { }

        // ✅ FACTORY METHOD - Only way to create valid Product
        public static Product Create(
            Guid tenantId,
            Guid categoryId,
            string name,
            string? description,
            SKU sku,
            Money price,
            ProductImages? images,
            string createdBy)
        {
            // Business validation
            ValidateName(name);

            ArgumentNullException.ThrowIfNull(sku, nameof(sku));
            ArgumentNullException.ThrowIfNull(price, nameof(price));

            // Create product
            var product = new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CategoryId = categoryId,
                _name = name,
                _description = description,
                _sku = sku,
                _price = price,
                _images = images,
                _status = ProductStatus.Draft  // Always starts as Draft
            };

            // ✅ Raise domain event
            product.AddDomainEvent(new ProductCreatedEvent(
                ProductId: product.Id,
                TenantId: tenantId,
                Name: name,
                SKU: sku.Value,
                Price: price.Amount
            ));

            return product;
        }

        // Business validation
        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty");

            if (name.Length > 200)
                throw new ArgumentException("Product name cannot exceed 200 characters");
        }

        // Helper to add events
        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    // Domain event
    public record ProductCreatedEvent(
        Guid ProductId,
        Guid TenantId,
        string Name,
        string SKU,
        decimal Price
    ) : IDomainEvent;

    public interface IDomainEvent { }

    public enum ProductStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Discontinued = 3
    }

    // Value Objects (simplified for example)
    public class SKU
    {
        public string Value { get; }
        private SKU(string value) => Value = value;
        public static SKU? Create(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new SKU(value.ToUpperInvariant());
    }

    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        public static Money? Create(decimal amount, string currency = "USD") =>
            amount < 0 ? null : new Money(amount, currency.ToUpperInvariant());
    }

    public class ProductImages
    {
        public IReadOnlyList<string> Urls { get; }
        private ProductImages(IReadOnlyList<string> urls) => Urls = urls;
        public static ProductImages? Create(IEnumerable<string> urls)
        {
            var list = urls?.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
            return list?.Any() == true ? new ProductImages(list) : null;
        }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                  LAYER 4: INFRASTRUCTURE LAYER                         ║
    // ║                                                                        ║
    // ║  Responsibilities:                                                     ║
    // ║  - Database access (EF Core)                                           ║
    // ║  - Repository implementations                                          ║
    // ║  - Unit of Work implementation                                         ║
    // ║  - External services                                                   ║
    // ║  - Event dispatching                                                   ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Repository Interface - Defined in DOMAIN, implemented in INFRASTRUCTURE
    ///
    /// This is Dependency Inversion:
    /// - Domain defines what it needs
    /// - Infrastructure provides it
    /// - Domain doesn't depend on infrastructure
    /// </summary>
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
        Task AddAsync(Product product, CancellationToken ct = default);
        void Remove(Product product);
    }

    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }

    /// <summary>
    /// Unit of Work - Coordinates repositories and transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }

    /// <summary>
    /// Repository Implementation - Uses EF Core
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;

        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        {
            var normalizedSku = sku.ToUpperInvariant();
            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU.Value == normalizedSku, ct);
        }

        public async Task AddAsync(Product product, CancellationToken ct = default)
        {
            await _context.Products.AddAsync(product, ct);
        }

        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }
    }

    /// <summary>
    /// Unit of Work Implementation
    ///
    /// This is the KEY coordinator:
    /// 1. Manages DbContext
    /// 2. Provides access to repositories
    /// 3. Dispatches domain events BEFORE saving
    /// 4. Saves all changes in single transaction
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CatalogDbContext _context;
        private readonly IMediator _mediator;
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;

        public UnitOfWork(
            CatalogDbContext context,
            IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
            _products = new ProductRepository(context);
            _categories = new CategoryRepository(context);
        }

        public IProductRepository Products => _products;
        public ICategoryRepository Categories => _categories;

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            // ✅ DISPATCH DOMAIN EVENTS BEFORE SAVING
            await DispatchDomainEventsAsync(ct);

            // ✅ Save all changes in single transaction
            return await _context.SaveChangesAsync(ct);
        }

        private async Task DispatchDomainEventsAsync(CancellationToken ct)
        {
            // Get all entities with domain events
            var entitiesWithEvents = _context.ChangeTracker
                .Entries<Product>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            // Get all domain events
            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear events from entities
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            // Dispatch events via MediatR
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, ct);
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    /// <summary>
    /// DbContext - EF Core database context
    /// </summary>
    public class CatalogDbContext : IDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            Task.FromResult(1);

        public IChangeTracker ChangeTracker { get; } = new ChangeTracker();

        public void Dispose() { }
    }

    // Simplified EF Core types for example
    public interface IDbContext : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }

    public class DbSet<T> where T : class
    {
        public async Task<T?> FirstOrDefaultAsync(
            System.Linq.Expressions.Expression<Func<T, bool>> predicate,
            CancellationToken ct = default) =>
            await Task.FromResult<T?>(null);

        public async Task AddAsync(T entity, CancellationToken ct = default) =>
            await Task.CompletedTask;

        public void Remove(T entity) { }
    }

    public class ChangeTracker
    {
        public IEnumerable<EntityEntry<T>> Entries<T>() where T : class =>
            Enumerable.Empty<EntityEntry<T>>();
    }

    public class EntityEntry<T> where T : class
    {
        public T Entity { get; set; }
    }

    public class Category
    {
        public Guid Id { get; set; }
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context;
        public CategoryRepository(CatalogDbContext context) => _context = context;
        public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<Category?>(new Category { Id = id });
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                       EVENT HANDLERS                                   ║
    // ║                                                                        ║
    // ║  React to domain events (side effects)                                 ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Event Handler - Reacts to ProductCreatedEvent
    ///
    /// This runs AUTOMATICALLY when product is created:
    /// - Updates search index
    /// - Sends notifications
    /// - Updates analytics
    /// etc.
    /// </summary>
    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ISearchIndexer _searchIndexer;
        private readonly INotificationService _notifications;

        public ProductCreatedEventHandler(
            ISearchIndexer searchIndexer,
            INotificationService notifications)
        {
            _searchIndexer = searchIndexer;
            _notifications = notifications;
        }

        public async Task Handle(ProductCreatedEvent evt, CancellationToken ct)
        {
            // Side effect 1: Add to search index
            await _searchIndexer.IndexProductAsync(evt.ProductId, evt.Name, ct);

            // Side effect 2: Send notification
            await _notifications.NotifyProductCreatedAsync(evt.ProductId, ct);

            // Easy to add more side effects - just add code here!
        }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                    SUPPORTING TYPES                                    ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    // Result pattern
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public string Error { get; }

        private Result(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) =>
            new Result<T>(true, value, string.Empty);

        public static Result<T> Failure(string error) =>
            new Result<T>(false, default!, error);
    }

    // MediatR interfaces (simplified)
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
        Task Publish(object notification, CancellationToken ct = default);
    }

    public interface IRequest<out TResponse> { }

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct);
    }

    public interface INotificationHandler<in TNotification>
    {
        Task Handle(TNotification notification, CancellationToken ct);
    }

    public interface IValidator<in T>
    {
        bool Validate(T instance, out List<string> errors);
    }

    public interface ICreateProductCommandValidator : IValidator<CreateProductCommand> { }

    // Infrastructure services
    public interface ISearchIndexer
    {
        Task IndexProductAsync(Guid productId, string name, CancellationToken ct);
    }

    public interface INotificationService
    {
        Task NotifyProductCreatedAsync(Guid productId, CancellationToken ct);
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                         FLOW SUMMARY                                   ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /*
     * COMPLETE FLOW:
     *
     * 1. HTTP POST /api/products
     *    │
     *    ├─> ProductsController.CreateProduct()
     *    │   └─> Maps HTTP request to CreateProductCommand
     *    │
     * 2. Send command via MediatR
     *    │
     *    ├─> CreateProductCommandHandler.Handle()
     *    │   ├─> Validates input
     *    │   ├─> Checks business rules (SKU unique, category exists)
     *    │   ├─> Creates value objects (SKU, Money, ProductImages)
     *    │   ├─> Calls Product.Create() [DOMAIN LAYER]
     *    │   │   └─> Creates Product entity
     *    │   │   └─> Raises ProductCreatedEvent
     *    │   ├─> Adds product to repository
     *    │   └─> Calls UnitOfWork.SaveChangesAsync()
     *    │
     * 3. Unit of Work saves
     *    │
     *    ├─> UnitOfWork.SaveChangesAsync()
     *    │   ├─> Dispatches domain events (ProductCreatedEvent)
     *    │   │   └─> ProductCreatedEventHandler.Handle()
     *    │   │       ├─> Indexes product in search
     *    │   │       └─> Sends notifications
     *    │   └─> Saves to database (EF Core)
     *    │
     * 4. Returns result
     *    │
     *    ├─> Returns Product ID
     *    └─> Maps to HTTP 201 Created response
     *
     * LAYER RESPONSIBILITIES:
     *
     * API Layer:
     * - HTTP concerns only
     * - Maps requests/responses
     * - Thin controllers
     *
     * Application Layer:
     * - Orchestrates use cases
     * - Validates inputs
     * - Coordinates infrastructure
     * - NO business logic
     *
     * Domain Layer:
     * - ALL business logic
     * - Entities, value objects
     * - Domain events
     * - No infrastructure dependencies
     *
     * Infrastructure Layer:
     * - Database access
     * - External services
     * - Repository implementations
     * - Event dispatching
     *
     * KEY PRINCIPLES:
     *
     * ✅ Dependency Direction: API → Application → Domain ← Infrastructure
     * ✅ Domain is pure (no infrastructure dependencies)
     * ✅ Each layer has single responsibility
     * ✅ Business logic centralized in domain
     * ✅ Side effects via domain events
     * ✅ Testable (can test each layer independently)
     */
}
