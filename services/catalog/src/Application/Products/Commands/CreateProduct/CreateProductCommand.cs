using MediatR;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Application.Products.DTOs;
using Vendo.CatalogManagement.Domain.Enums;

namespace Vendo.CatalogManagement.Application.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product.
/// </summary>
public class CreateProductCommand : IRequest<Result<ProductDto>>
{
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public bool TrackInventory { get; set; } = true;
    public bool IsTaxable { get; set; } = true;
    public decimal TaxRate { get; set; } = 0;
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? AdditionalImages { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public bool IsFeatured { get; set; }
    public List<string>? Tags { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public int DisplayOrder { get; set; }
}
