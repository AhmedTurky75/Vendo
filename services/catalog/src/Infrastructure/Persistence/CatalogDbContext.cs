using Microsoft.EntityFrameworkCore;
using Vendo.CatalogManagement.Domain.Entities;

namespace Vendo.CatalogManagement.Infrastructure.Persistence;

/// <summary>
/// Database context for the Catalog service.
/// </summary>
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
