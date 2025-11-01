using Vendo.CatalogManagement.Domain.Common;
using Vendo.CatalogManagement.Domain.Events;
using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Domain.Entities;

/// <summary>
/// Category aggregate root.
/// Represents a product category with support for hierarchical structures.
/// </summary>
public class Category : BaseEntity
{
    private Slug _slug;
    private readonly List<Category> _childCategories = new();
    private readonly List<Product> _products = new();

    // Properties with private setters for encapsulation
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Slug Slug => _slug;
    public Guid? ParentCategoryId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation properties
    public Category? ParentCategory { get; private set; }
    public IReadOnlyCollection<Category> ChildCategories => _childCategories.AsReadOnly();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Private constructor for EF Core
    private Category() { }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    public static Category Create(
        Guid tenantId,
        string name,
        string? description,
        Guid? parentCategoryId,
        string createdBy)
    {
        ValidateName(name);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            _slug = Slug.Generate(name) ?? throw new InvalidOperationException("Failed to generate slug"),
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            DisplayOrder = 0
        };

        category.SetCreatedAudit(createdBy);

        // Raise domain event
        category.AddDomainEvent(new CategoryCreatedEvent(
            category.Id,
            tenantId,
            name,
            parentCategoryId));

        return category;
    }

    /// <summary>
    /// Updates the category information.
    /// </summary>
    public void UpdateInformation(
        string name,
        string? description,
        string updatedBy)
    {
        ValidateName(name);

        var nameChanged = Name != name.Trim();

        Name = name.Trim();
        Description = description?.Trim();

        // Regenerate slug if name changed
        if (nameChanged)
        {
            _slug = Slug.Generate(name) ?? _slug;
        }

        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Changes the parent category.
    /// </summary>
    public void ChangeParent(Guid? parentCategoryId, string updatedBy)
    {
        // Prevent setting self as parent
        if (parentCategoryId == Id)
            throw new InvalidOperationException("Category cannot be its own parent");

        ParentCategoryId = parentCategoryId;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Activates the category.
    /// </summary>
    public void Activate(string updatedBy)
    {
        if (IsActive)
            return;

        IsActive = true;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Deactivates the category.
    /// </summary>
    public void Deactivate(string updatedBy)
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the category image.
    /// </summary>
    public void SetImage(string? imageUrl, string updatedBy)
    {
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            // Basic URL validation
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                throw new ArgumentException("Invalid image URL", nameof(imageUrl));

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("Image URL must use HTTP or HTTPS protocol", nameof(imageUrl));
        }

        ImageUrl = imageUrl;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int order, string updatedBy)
    {
        if (order < 0)
            throw new ArgumentException("Display order cannot be negative", nameof(order));

        DisplayOrder = order;
        SetUpdatedAudit(updatedBy);
    }

    /// <summary>
    /// Checks if this category is a root category (has no parent).
    /// </summary>
    public bool IsRootCategory() => !ParentCategoryId.HasValue;

    /// <summary>
    /// Checks if this category has child categories.
    /// </summary>
    public bool HasChildren() => _childCategories.Any();

    /// <summary>
    /// Checks if this category has products.
    /// </summary>
    public bool HasProducts() => _products.Any();

    /// <summary>
    /// Gets the total number of products (including child categories).
    /// </summary>
    public int GetTotalProductCount()
    {
        var count = _products.Count;
        foreach (var child in _childCategories)
        {
            count += child.GetTotalProductCount();
        }
        return count;
    }

    /// <summary>
    /// Validates if the category can be deleted.
    /// </summary>
    public bool CanBeDeleted()
    {
        // Category can be deleted if it has no products and no child categories
        return !HasProducts() && !HasChildren();
    }

    // Private validation methods
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters", nameof(name));
    }
}
