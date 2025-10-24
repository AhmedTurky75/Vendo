using Vendo.Catalog.Domain.Enums;

namespace Vendo.Catalog.Application.Products.DTOs;

/// <summary>
/// Product data transfer object.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsTaxable { get; set; }
    public decimal TaxRate { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? AdditionalImages { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public List<string>? Tags { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
