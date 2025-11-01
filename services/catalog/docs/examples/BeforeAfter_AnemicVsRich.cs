// ============================================================================
// EXAMPLE: Transforming Anemic Domain Model to Rich Domain Model
// ============================================================================
//
// This example shows a complete transformation from an anemic model
// (data container with no behavior) to a rich domain model (behavior + data).
//
// KEY LEARNING:
// - Anemic models push business logic to service layer
// - Rich models encapsulate business logic in the domain
// - Rich models are easier to test, maintain, and reason about
//
// ============================================================================

using System;
using System.Collections.Generic;

namespace Vendo.CatalogManagement.Examples
{
    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                           BEFORE: ANEMIC MODEL                         ║
    // ║                           (Anti-Pattern)                               ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// ❌ ANEMIC MODEL: Just a data container
    /// Problems:
    /// - No behavior, just getters/setters
    /// - Business logic lives elsewhere (in services)
    /// - Easy to put entity in invalid state
    /// - No encapsulation
    /// </summary>
    public class Product_Anemic
    {
        // ❌ Everything public with setters - no protection!
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }              // Can be negative!
        public int StockQuantity { get; set; }          // Can be negative!
        public ProductStatus Status { get; set; }       // Can bypass business rules!
        public bool HasImages { get; set; }
    }

    public enum ProductStatus
    {
        Draft,
        Active,
        Discontinued
    }

    /// <summary>
    /// ❌ ANEMIC APPROACH: Business logic in service layer
    /// Problems:
    /// - Domain knowledge scattered across service classes
    /// - Hard to find all business rules for Product
    /// - Duplicate validation across services
    /// - Can't ensure rules are always followed
    /// </summary>
    public class ProductService_Anemic
    {
        private readonly IProductRepository_Anemic _repository;

        public ProductService_Anemic(IProductRepository_Anemic repository)
        {
            _repository = repository;
        }

        // Business logic HERE instead of in domain
        public async Task ChangePrice(Guid productId, decimal newPrice)
        {
            // ❌ Validation in service layer (should be in domain)
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative");

            var product = await _repository.GetByIdAsync(productId);

            // ❌ Direct property setter - no encapsulation
            product.Price = newPrice;

            await _repository.SaveChangesAsync();

            // ❌ Manually trigger side effects (easy to forget!)
            await NotifyPriceChangeAsync(productId, newPrice);
            await UpdateSearchIndexAsync(productId);
            await InvalidateCacheAsync(productId);
        }

        // More business logic in service
        public async Task PublishProduct(Guid productId)
        {
            var product = await _repository.GetByIdAsync(productId);

            // ❌ Business rules in service (should be in domain)
            if (!product.HasImages)
                throw new InvalidOperationException("Cannot publish product without images");

            if (product.Price <= 0)
                throw new InvalidOperationException("Cannot publish product without valid price");

            // ❌ Direct property change
            product.Status = ProductStatus.Active;

            await _repository.SaveChangesAsync();

            // ❌ More manual side effects
            await NotifyProductPublishedAsync(productId);
            await AddToSearchIndexAsync(productId);
        }

        public async Task UpdateStock(Guid productId, int newQuantity)
        {
            // ❌ Validation scattered
            if (newQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative");

            var product = await _repository.GetByIdAsync(productId);
            product.StockQuantity = newQuantity;  // Direct setter

            await _repository.SaveChangesAsync();

            // ❌ Easy to forget this notification
            if (newQuantity == 0)
                await NotifyOutOfStockAsync(productId);
        }

        // Dummy methods for example
        private Task NotifyPriceChangeAsync(Guid id, decimal price) => Task.CompletedTask;
        private Task UpdateSearchIndexAsync(Guid id) => Task.CompletedTask;
        private Task InvalidateCacheAsync(Guid id) => Task.CompletedTask;
        private Task NotifyProductPublishedAsync(Guid id) => Task.CompletedTask;
        private Task AddToSearchIndexAsync(Guid id) => Task.CompletedTask;
        private Task NotifyOutOfStockAsync(Guid id) => Task.CompletedTask;
    }

    public interface IProductRepository_Anemic
    {
        Task<Product_Anemic> GetByIdAsync(Guid id);
        Task SaveChangesAsync();
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                         AFTER: RICH DOMAIN MODEL                       ║
    // ║                            (Best Practice)                             ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// ✅ RICH DOMAIN MODEL: Behavior + Data
    /// Benefits:
    /// - Business logic encapsulated in entity
    /// - Impossible to create invalid state
    /// - Easy to test (just test entity methods)
    /// - Self-documenting (methods show what's possible)
    /// - Domain events for side effects
    /// </summary>
    public class Product_Rich
    {
        // ✅ Private setters - only entity can modify itself
        public Guid Id { get; private set; }

        // ✅ Private backing fields for value objects
        private string _name;
        private decimal _price;
        private int _stockQuantity;
        private ProductStatus _status;
        private List<string> _imageUrls = new();

        // ✅ Domain events collection
        private readonly List<object> _domainEvents = new();
        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        // ✅ Private constructor - can't create directly
        private Product_Rich() { }

        // ✅ FACTORY METHOD: Only way to create valid product
        public static Product_Rich Create(
            string name,
            decimal price,
            int initialStock)
        {
            // Validation at creation
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (initialStock < 0)
                throw new ArgumentException("Stock quantity cannot be negative");

            var product = new Product_Rich
            {
                Id = Guid.NewGuid(),
                _name = name,
                _price = price,
                _stockQuantity = initialStock,
                _status = ProductStatus.Draft  // Always starts as Draft
            };

            // ✅ Raise domain event
            product.AddDomainEvent(new ProductCreatedEvent(product.Id, name));

            return product;
        }

        // ✅ BUSINESS METHOD: Encapsulates price change logic
        public void ChangePrice(decimal newPrice)
        {
            // ✅ Validation in domain
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative");

            var oldPrice = _price;
            _price = newPrice;

            // ✅ Raise domain event - no manual side effects needed!
            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
        }

        // ✅ BUSINESS METHOD: Encapsulates publish logic
        public void Publish()
        {
            // ✅ Business rules enforced in domain
            if (!_imageUrls.Any())
                throw new InvalidOperationException("Cannot publish product without images");

            if (_price <= 0)
                throw new InvalidOperationException("Cannot publish product without valid price");

            _status = ProductStatus.Active;

            // ✅ Raise domain event
            AddDomainEvent(new ProductPublishedEvent(Id, _name));
        }

        // ✅ BUSINESS METHOD: Encapsulates stock update logic
        public void UpdateStock(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative");

            var oldQuantity = _stockQuantity;
            _stockQuantity = newQuantity;

            // ✅ Raise event
            AddDomainEvent(new ProductStockChangedEvent(Id, oldQuantity, newQuantity));

            // ✅ Business logic: Check if out of stock
            if (newQuantity == 0 && oldQuantity > 0)
                AddDomainEvent(new ProductOutOfStockEvent(Id));
        }

        // ✅ BUSINESS METHOD: Add image
        public void AddImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL cannot be empty");

            if (_imageUrls.Contains(imageUrl))
                return;  // Already exists

            _imageUrls.Add(imageUrl);
        }

        // ✅ QUERY METHOD: Check stock status
        public bool IsOutOfStock() => _stockQuantity == 0 && _status == ProductStatus.Active;

        // ✅ QUERY METHOD: Check if can be published
        public bool CanBePublished() => _imageUrls.Any() && _price > 0;

        // Helper for events
        private void AddDomainEvent(object domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    /// <summary>
    /// ✅ THIN APPLICATION LAYER: Just orchestration
    /// Benefits:
    /// - Handlers are simple - just call domain methods
    /// - Business logic in domain, easy to find
    /// - Domain events trigger side effects automatically
    /// - Easy to test (mock repository, test domain separately)
    /// </summary>
    public class ProductCommandHandler_Rich
    {
        private readonly IProductRepository_Rich _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductCommandHandler_Rich(
            IProductRepository_Rich repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        // ✅ Thin handler - just orchestration
        public async Task ChangePrice(Guid productId, decimal newPrice)
        {
            var product = await _repository.GetByIdAsync(productId);

            // ✅ Call domain method - business logic in domain!
            product.ChangePrice(newPrice);

            // ✅ Unit of Work handles save and event dispatching
            await _unitOfWork.SaveChangesAsync();

            // ✅ No manual side effects - events handle them!
        }

        // ✅ Thin handler
        public async Task PublishProduct(Guid productId)
        {
            var product = await _repository.GetByIdAsync(productId);

            // ✅ Domain enforces business rules
            product.Publish();  // Throws if invalid

            await _unitOfWork.SaveChangesAsync();
            // ✅ Events automatically dispatched
        }

        // ✅ Thin handler
        public async Task UpdateStock(Guid productId, int newQuantity)
        {
            var product = await _repository.GetByIdAsync(productId);

            product.UpdateStock(newQuantity);  // Domain logic

            await _unitOfWork.SaveChangesAsync();
            // ✅ Out of stock notification via event handler
        }
    }

    /// <summary>
    /// ✅ EVENT HANDLERS: React to domain events (decoupled)
    /// Benefits:
    /// - Side effects decoupled from main logic
    /// - Easy to add new reactions (just add handler)
    /// - Can be tested independently
    /// </summary>
    public class ProductPriceChangedEventHandler
    {
        private readonly IPriceHistoryService _priceHistory;
        private readonly ISearchIndexer _searchIndexer;
        private readonly ICacheService _cache;

        public ProductPriceChangedEventHandler(
            IPriceHistoryService priceHistory,
            ISearchIndexer searchIndexer,
            ICacheService cache)
        {
            _priceHistory = priceHistory;
            _searchIndexer = searchIndexer;
            _cache = cache;
        }

        public async Task Handle(ProductPriceChangedEvent evt)
        {
            // All side effects here - not in main logic!
            await _priceHistory.RecordPriceChange(evt.ProductId, evt.OldPrice, evt.NewPrice);
            await _searchIndexer.UpdateProductPrice(evt.ProductId, evt.NewPrice);
            await _cache.InvalidateProduct(evt.ProductId);
        }
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                          DOMAIN EVENTS                                 ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    public record ProductCreatedEvent(Guid ProductId, string Name);
    public record ProductPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice);
    public record ProductPublishedEvent(Guid ProductId, string Name);
    public record ProductStockChangedEvent(Guid ProductId, int OldQuantity, int NewQuantity);
    public record ProductOutOfStockEvent(Guid ProductId);

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                        INTERFACES                                      ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    public interface IProductRepository_Rich
    {
        Task<Product_Rich> GetByIdAsync(Guid id);
        Task AddAsync(Product_Rich product);
        // ✅ No SaveChanges - that's Unit of Work's job
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();  // ✅ Dispatches events automatically
    }

    public interface IPriceHistoryService
    {
        Task RecordPriceChange(Guid productId, decimal oldPrice, decimal newPrice);
    }

    public interface ISearchIndexer
    {
        Task UpdateProductPrice(Guid productId, decimal price);
    }

    public interface ICacheService
    {
        Task InvalidateProduct(Guid productId);
    }

    // ╔═══════════════════════════════════════════════════════════════════════╗
    // ║                        COMPARISON SUMMARY                              ║
    // ╚═══════════════════════════════════════════════════════════════════════╝

    /*
     * COMPARISON:
     *
     * ┌──────────────────────┬─────────────────────┬─────────────────────┐
     * │ Aspect               │ Anemic Model ❌     │ Rich Model ✅        │
     * ├──────────────────────┼─────────────────────┼─────────────────────┤
     * │ Business Logic       │ Service layer       │ Domain entities     │
     * │ Validation           │ Scattered           │ Centralized         │
     * │ Invalid State        │ Possible            │ Impossible          │
     * │ Testability          │ Hard (need mocks)   │ Easy (test entity)  │
     * │ Side Effects         │ Manual              │ Automatic (events)  │
     * │ Encapsulation        │ None                │ Full                │
     * │ Maintainability      │ Low                 │ High                │
     * │ Understanding        │ Hard (logic spread) │ Easy (in entity)    │
     * └──────────────────────┴─────────────────────┴─────────────────────┘
     *
     * KEY BENEFITS OF RICH MODEL:
     *
     * 1. ENCAPSULATION
     *    - Private setters prevent direct modification
     *    - Business methods are the only way to change state
     *
     * 2. VALIDATION
     *    - All validation in domain (one place)
     *    - Impossible to create invalid state
     *
     * 3. TESTABILITY
     *    - Test domain logic without infrastructure
     *    - No need to mock repositories in domain tests
     *
     * 4. MAINTAINABILITY
     *    - All Product logic in Product class
     *    - Easy to find and modify business rules
     *
     * 5. DOMAIN EVENTS
     *    - Side effects decoupled
     *    - Easy to add new reactions
     *
     * HOW TO REFACTOR:
     *
     * Step 1: Make setters private
     * Step 2: Add business methods for each operation
     * Step 3: Move validation from services to domain methods
     * Step 4: Add domain events
     * Step 5: Thin down service layer to just orchestration
     * Step 6: Move side effects to event handlers
     */
}
