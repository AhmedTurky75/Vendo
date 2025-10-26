using Microsoft.EntityFrameworkCore;
using Vendo.PaymentManagement.Domain.Entities;
using Vendo.PaymentManagement.Infrastructure.Persistence.Configurations;

namespace Vendo.PaymentManagement.Infrastructure.Persistence;

/// <summary>
/// Database context for payment service.
/// </summary>
public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Payments DbSet.
    /// </summary>
    public DbSet<Domain.Entities.Payment> Payments => Set<Domain.Entities.Payment>();

    /// <summary>
    /// Gets or sets the Transactions DbSet.
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
    }
}
