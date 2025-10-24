using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.Payment.Domain.Enums;

namespace Vendo.Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Payment entity.
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("IX_Payments_TenantId");

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("IX_Payments_OrderId");

        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.HasIndex(p => p.CustomerId)
            .HasDatabaseName("IX_Payments_CustomerId");

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payments_Status");

        builder.Property(p => p.PaymentDate);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);

        builder.HasIndex(p => p.TransactionId)
            .HasDatabaseName("IX_Payments_TransactionId");

        builder.Property(p => p.GatewayResponse)
            .HasMaxLength(2000);

        builder.Property(p => p.RefundAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.RefundReason)
            .HasMaxLength(500);

        builder.Property(p => p.RefundDate);

        builder.Property(p => p.Metadata)
            .HasMaxLength(4000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();
    }
}
