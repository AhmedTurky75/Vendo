using Vendo.CatalogManagement.Domain.Common;

namespace Vendo.CatalogManagement.Domain.Entities;

/// <summary>
/// Represents a product category.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Gets or sets the tenant/store ID this category belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the category slug (URL-friendly name).
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent category ID (for hierarchical categories).
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Gets or sets the parent category navigation property.
    /// </summary>
    public Category? ParentCategory { get; set; }

    /// <summary>
    /// Gets or sets the child categories.
    /// </summary>
    public ICollection<Category> ChildCategories { get; set; } = new List<Category>();

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets whether the category is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the category image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the products in this category.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
