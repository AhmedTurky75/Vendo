using Microsoft.EntityFrameworkCore;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Infrastructure.Persistence.Configurations;

namespace Vendo.TenantManagement.Infrastructure.Persistence;

/// <summary>
/// Database context for tenant management.
/// </summary>
public class TenantManagementDbContext : DbContext
{
    public TenantManagementDbContext(DbContextOptions<TenantManagementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Stores DbSet.
    /// </summary>
    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new StoreConfiguration());
    }
}
