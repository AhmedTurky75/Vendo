using Microsoft.EntityFrameworkCore;
using Vendo.CatalogManagement.Domain.Entities;
using Vendo.CatalogManagement.Domain.Interfaces;
using Vendo.CatalogManagement.Infrastructure.Persistence;

namespace Vendo.CatalogManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Category operations.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ChildCategories)
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetChildCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid tenantId, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Where(c => c.Slug == slug && c.TenantId == tenantId);

        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }
}
