using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vendo.Order.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Entities.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderId)
            .IsRequired();

        builder.Property(o => o.TenantId)
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.CustomerEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.BillingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.SubTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        builder.Property(o => o.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.UpdatedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.TenantId);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.Status);
    }
}
