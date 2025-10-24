using Vendo.Catalog.Domain.Entities;

namespace Vendo.Catalog.Domain.Interfaces;

/// <summary>
/// Repository interface for Category operations.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by slug.
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all categories for a tenant.
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets root categories (no parent) for a tenant.
    /// </summary>
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets child categories of a parent category.
    /// </summary>
    Task<IReadOnlyList<Category>> GetChildCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category with its products.
    /// </summary>
    Task<Category?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a slug exists for a tenant.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid tenantId, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new category.
    /// </summary>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    void Update(Category category);

    /// <summary>
    /// Deletes a category.
    /// </summary>
    void Delete(Category category);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
