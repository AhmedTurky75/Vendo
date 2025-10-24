using Vendo.Catalog.Domain.Entities;

namespace Vendo.Catalog.Domain.Interfaces;

/// <summary>
/// Repository interface for Product operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by slug.
    /// </summary>
    Task<Product?> GetBySlugAsync(string slug, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products for a tenant with pagination.
    /// </summary>
    Task<IReadOnlyList<Product>> GetAllAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products by category.
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets featured products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(Guid tenantId, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name or description.
    /// </summary>
    Task<IReadOnlyList<Product>> SearchAsync(Guid tenantId, string searchTerm, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of products for a tenant.
    /// </summary>
    Task<int> GetCountAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a SKU exists for a tenant.
    /// </summary>
    Task<bool> SkuExistsAsync(string sku, Guid tenantId, Guid? excludeProductId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new product.
    /// </summary>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    void Update(Product product);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    void Delete(Product product);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
