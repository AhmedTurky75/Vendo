using Vendo.Catalog.Application.Products.DTOs;

namespace Vendo.Catalog.Application.Categories.DTOs;

/// <summary>
/// Category with products data transfer object.
/// </summary>
public class CategoryWithProductsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}
