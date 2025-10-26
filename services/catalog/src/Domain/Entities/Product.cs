using Vendo.CatalogManagement.Domain.Common;
using Vendo.CatalogManagement.Domain.Enums;

namespace Vendo.CatalogManagement.Domain.Entities;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Gets or sets the tenant/store ID this product belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category navigation property.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the short description.
    /// </summary>
    public string? ShortDescription { get; set; }

    /// <summary>
    /// Gets or sets the product SKU (Stock Keeping Unit).
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product slug (URL-friendly name).
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the compare at price (original price before discount).
    /// </summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary>
    /// Gets or sets the cost price.
    /// </summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Gets or sets the stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the low stock threshold.
    /// </summary>
    public int LowStockThreshold { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to track inventory.
    /// </summary>
    public bool TrackInventory { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the product is taxable.
    /// </summary>
    public bool IsTaxable { get; set; } = true;

    /// <summary>
    /// Gets or sets the tax rate percentage.
    /// </summary>
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// Gets or sets the product weight (in kg).
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Gets or sets the product dimensions (LxWxH in cm).
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Gets or sets the main product image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets additional product images (JSON array of URLs).
    /// </summary>
    public string? AdditionalImages { get; set; }

    /// <summary>
    /// Gets or sets the product status.
    /// </summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    /// <summary>
    /// Gets or sets whether the product is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Gets or sets the product tags (comma-separated).
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the meta title for SEO.
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Gets or sets the meta description for SEO.
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// Gets or sets the meta keywords for SEO.
    /// </summary>
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Gets or sets the display order within category.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the number of views.
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the number of sales.
    /// </summary>
    public int SalesCount { get; set; }
}
