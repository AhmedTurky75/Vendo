using Microsoft.EntityFrameworkCore;
using Vendo.Catalog.Domain.Entities;
using Vendo.Catalog.Domain.Interfaces;
using Vendo.Catalog.Infrastructure.Persistence;

namespace Vendo.Catalog.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Product operations.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.SKU == sku && p.TenantId == tenantId, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(Guid tenantId, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.TenantId == tenantId && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(Guid tenantId, string searchTerm, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.TenantId == tenantId &&
                       (p.Name.Contains(searchTerm) ||
                        (p.Description != null && p.Description.Contains(searchTerm)) ||
                        p.SKU.Contains(searchTerm)))
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .CountAsync(p => p.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid tenantId, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Where(p => p.SKU == sku && p.TenantId == tenantId);

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
