using Microsoft.EntityFrameworkCore;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.Interfaces;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Store entity.
/// </summary>
public class StoreRepository : IStoreRepository
{
    private readonly TenantManagementDbContext _context;

    public StoreRepository(TenantManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Store?> GetBySubdomainAsync(Subdomain subdomain, CancellationToken cancellationToken = default)
    {
        return await GetBySubdomainAsync(subdomain.Value, cancellationToken);
    }

    public async Task<Store?> GetBySubdomainAsync(string subdomainString, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.Subdomain == Subdomain.Create(subdomainString).Value!, cancellationToken);
    }

    public async Task<IReadOnlyList<Store>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .Where(s => s.OwnerId == ownerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Store>> GetAllAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var subdomainLower = subdomain.ToLowerInvariant();
        return await _context.Stores
            .AnyAsync(s => EF.Functions.Like(s.Subdomain.ToString(), subdomainLower), cancellationToken);
    }

    public async Task AddAsync(Store store, CancellationToken cancellationToken = default)
    {
        await _context.Stores.AddAsync(store, cancellationToken);
    }

    public void Update(Store store)
    {
        _context.Stores.Update(store);
    }

    public void Delete(Store store)
    {
        _context.Stores.Remove(store);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
