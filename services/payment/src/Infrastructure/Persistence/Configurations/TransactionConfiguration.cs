using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.PaymentManagement.Domain.Entities;
using Vendo.PaymentManagement.Domain.Enums;

namespace Vendo.PaymentManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Transaction entity.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.PaymentId)
            .IsRequired();

        builder.HasIndex(t => t.PaymentId)
            .HasDatabaseName("IX_Transactions_PaymentId");

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.GatewayTransactionId)
            .HasMaxLength(200);

        builder.Property(t => t.GatewayResponse)
            .HasMaxLength(2000);

        builder.Property(t => t.ProcessedAt)
            .IsRequired();
    }
}
