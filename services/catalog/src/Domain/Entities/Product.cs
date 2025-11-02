using Vendo.CatalogManagement.Domain.Common;
using Vendo.CatalogManagement.Domain.Enums;
using Vendo.CatalogManagement.Domain.Events;
using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Domain.Entities;

/// <summary>
/// Product aggregate root.
/// Encapsulates all product-related business rules and invariants.
/// </summary>
public class Product : BaseEntity
{
    // Private backing fields for encapsulation
    private Money _price;
    private Money? _compareAtPrice;
    private Money? _costPrice;
    private SKU _sku;
    private Slug _slug;
    private int _stockQuantity;
    private int _lowStockThreshold;
    private ProductStatus _status;
    private readonly List<string> _tags = new();

    // Properties with private setters for encapsulation
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ShortDescription { get; private set; }

    // Expose value objects as properties
    public SKU SKU => _sku;
    public Slug Slug => _slug;
    public Money Price => _price;
    public Money? CompareAtPrice => _compareAtPrice;
    public Money? CostPrice => _costPrice;

    public int StockQuantity => _stockQuantity;
    public int LowStockThreshold => _lowStockThreshold;
    public bool TrackInventory { get; private set; }
    public bool IsTaxable { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal? Weight { get; private set; }
    public Dimensions? Dimensions { get; private set; }
    public ProductImages Images { get; private set; } = ProductImages.Empty();
    public ProductStatus Status => _status;
    public bool IsFeatured { get; private set; }
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public SEOMetadata SEOMetadata { get; private set; } = SEOMetadata.Empty();
    public int DisplayOrder { get; private set; }
    public int ViewCount { get; private set; }
    public int SalesCount { get; private set; }

    // Navigation property
    public Category? Category { get; private set; }

    // Private constructor for EF Core
    private Product() { }

    // Factory method for creating new products
    /// <summary>
    /// Creates a new product.
    /// </summary>
    public static Product Create(
        Guid tenantId,
        Guid categoryId,
        string name,
        string? description,
        SKU sku,
        Money price,
        string createdBy)
    {
        ValidateName(name);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CategoryId = categoryId,
            Name = name.Trim(),
            Description = description?.Trim(),
            _sku = sku,
            _slug = Slug.Generate(name) ?? throw new InvalidOperationException("Failed to generate slug"),
            _price = price,
            _status = ProductStatus.Draft,
            _stockQuantity = 0,
            _lowStockThreshold = 10,
            TrackInventory = true,
            IsTaxable = true,
            TaxRate = 0
        };

        product.SEOMetadata = SEOMetadata.FromContent(name, description);
        product.SetCreatedAudit(createdBy);

        // Raise domain event
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            tenantId,
            categoryId,
            name,
            sku.Value));

        return product;
    }

    // Business methods for modifying the product

    /// <summary>
    /// Updates the product information.
    /// </summary>
    public void UpdateInformation(
        string name,
        string? description,
        string? shortDescription,
        string updatedBy)
    {
        ValidateName(name);

        var nameChanged = Name != name.Trim();

        Name = name.Trim();
        Description = description?.Trim();
        ShortDescription = shortDescription?.Trim();

        // Regenerate slug if name changed
        if (nameChanged)
        {
            _slug = Slug.Generate(name) ?? _slug;
        }

        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Changes the product price.
    /// </summary>
    public void ChangePrice(Money newPrice, string updatedBy)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));

        var oldPrice = _price.Amount;
        _price = newPrice;

        SetUpdatedAudit(updatedBy);

        // Raise domain event
        AddDomainEvent(new ProductPriceChangedEvent(
            Id,
            oldPrice,
            newPrice.Amount,
            newPrice.Currency));
    }

    /// <summary>
    /// Sets the compare at price (original price before discount).
    /// </summary>
    public void SetCompareAtPrice(Money? compareAtPrice, string updatedBy)
    {
        if (compareAtPrice != null && compareAtPrice <= _price)
            throw new InvalidOperationException("Compare at price must be greater than current price");

        _compareAtPrice = compareAtPrice;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the cost price.
    /// </summary>
    public void SetCostPrice(Money? costPrice, string updatedBy)
    {
        if (costPrice != null && costPrice >= _price)
            throw new InvalidOperationException("Cost price should be less than selling price");

        _costPrice = costPrice;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Updates the stock quantity.
    /// </summary>
    public void UpdateStock(int quantity, string updatedBy)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        var oldQuantity = _stockQuantity;
        _stockQuantity = quantity;

        SetUpdatedAudit(updatedBy);

        // Raise domain event
        var isLowStock = IsLowStock();
        AddDomainEvent(new ProductStockChangedEvent(
            Id,
            oldQuantity,
            quantity,
            isLowStock));
    }

    /// <summary>
    /// Increases stock quantity.
    /// </summary>
    public void IncreaseStock(int amount, string updatedBy)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        UpdateStock(_stockQuantity + amount, updatedBy);
    }

    /// <summary>
    /// Decreases stock quantity.
    /// </summary>
    public void DecreaseStock(int amount, string updatedBy)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (_stockQuantity < amount)
            throw new InvalidOperationException("Insufficient stock");

        UpdateStock(_stockQuantity - amount, updatedBy);
    }

    /// <summary>
    /// Sets the low stock threshold.
    /// </summary>
    public void SetLowStockThreshold(int threshold, string updatedBy)
    {
        if (threshold < 0)
            throw new ArgumentException("Threshold cannot be negative", nameof(threshold));

        _lowStockThreshold = threshold;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Checks if the product is low on stock.
    /// </summary>
    public bool IsLowStock()
    {
        return TrackInventory && _stockQuantity <= _lowStockThreshold;
    }

    /// <summary>
    /// Checks if the product is out of stock.
    /// </summary>
    public bool IsOutOfStock()
    {
        return TrackInventory && _stockQuantity == 0;
    }

    /// <summary>
    /// Changes the product category.
    /// </summary>
    public void ChangeCategory(Guid categoryId, string updatedBy)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

        CategoryId = categoryId;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Publishes the product (makes it active).
    /// </summary>
    public void Publish(string updatedBy)
    {
        if (_status == ProductStatus.Active)
            return;

        ValidateForPublishing();

        _status = ProductStatus.Active;
        SetUpdatedAudit(updatedBy);

        // Raise domain event
        AddDomainEvent(new ProductPublishedEvent(Id, TenantId, Name));
    }

    /// <summary>
    /// Unpublishes the product (makes it inactive).
    /// </summary>
    public void Unpublish(string updatedBy)
    {
        _status = ProductStatus.Inactive;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the product as draft.
    /// </summary>
    public void SetDraft(string updatedBy)
    {
        _status = ProductStatus.Draft;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Archives the product.
    /// </summary>
    public void Archive(string updatedBy)
    {
        _status = ProductStatus.Archived;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets whether the product is featured.
    /// </summary>
    public void SetFeatured(bool featured, string updatedBy)
    {
        IsFeatured = featured;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Adds a tag to the product.
    /// </summary>
    public void AddTag(string tag, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalizedTag = tag.Trim().ToLowerInvariant();

        if (_tags.Contains(normalizedTag))
            return;

        _tags.Add(normalizedTag);
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Removes a tag from the product.
    /// </summary>
    public void RemoveTag(string tag, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalizedTag = tag.Trim().ToLowerInvariant();
        _tags.Remove(normalizedTag);
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the product images.
    /// </summary>
    public void SetImages(ProductImages images, string updatedBy)
    {
        Images = images ?? ProductImages.Empty();
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the SEO metadata.
    /// </summary>
    public void SetSEOMetadata(SEOMetadata seoMetadata, string updatedBy)
    {
        SEOMetadata = seoMetadata ?? SEOMetadata.Empty();
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the product dimensions.
    /// </summary>
    public void SetDimensions(Dimensions? dimensions, string updatedBy)
    {
        Dimensions = dimensions;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the product weight.
    /// </summary>
    public void SetWeight(decimal? weight, string updatedBy)
    {
        if (weight.HasValue && weight.Value < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(weight));

        Weight = weight;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the tax configuration.
    /// </summary>
    public void SetTaxConfiguration(bool isTaxable, decimal taxRate, string updatedBy)
    {
        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Tax rate must be between 0 and 100", nameof(taxRate));

        IsTaxable = isTaxable;
        TaxRate = taxRate;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the inventory tracking.
    /// </summary>
    public void SetInventoryTracking(bool trackInventory, string updatedBy)
    {
        TrackInventory = trackInventory;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int order, string updatedBy)
    {
        DisplayOrder = order;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Increments the view count.
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
    }

    /// <summary>
    /// Increments the sales count.
    /// </summary>
    public void IncrementSalesCount()
    {
        SalesCount++;
    }

    /// <summary>
    /// Calculates the discount percentage.
    /// </summary>
    public decimal? CalculateDiscountPercentage()
    {
        if (_compareAtPrice == null || _compareAtPrice <= _price)
            return null;

        var discount = ((_compareAtPrice.Amount - _price.Amount) / _compareAtPrice.Amount) * 100;
        return Math.Round(discount, 2);
    }

    /// <summary>
    /// Calculates the profit margin.
    /// </summary>
    public decimal? CalculateProfitMargin()
    {
        if (_costPrice == null || _costPrice >= _price)
            return null;

        var margin = ((_price.Amount - _costPrice.Amount) / _price.Amount) * 100;
        return Math.Round(margin, 2);
    }

    // Private validation methods
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters", nameof(name));
    }

    private void ValidateForPublishing()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Cannot publish product without a name");

        if (_price == null || _price.Amount <= 0)
            throw new InvalidOperationException("Cannot publish product with invalid price");

        if (CategoryId == Guid.Empty)
            throw new InvalidOperationException("Cannot publish product without a category");
    }
}
