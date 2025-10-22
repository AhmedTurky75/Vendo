using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.TenantManagement.Domain.Entities;
using Vendo.TenantManagement.Domain.Enums;
using Vendo.TenantManagement.Domain.ValueObjects;

namespace Vendo.TenantManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Store entity.
/// </summary>
public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Subdomain value object
        builder.Property(s => s.Subdomain)
            .IsRequired()
            .HasMaxLength(63)
            .HasConversion(
                subdomain => subdomain.Value,
                value => Subdomain.Create(value).Value!)
            .HasColumnName("Subdomain");

        builder.HasIndex(s => s.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Stores_Subdomain");

        builder.Property(s => s.OwnerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(s => s.OwnerId)
            .HasDatabaseName("IX_Stores_OwnerId");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.SubscriptionTier)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.TrialEndsAt);
        builder.Property(s => s.SubscriptionRenewedAt);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(450);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(450);

        // Configure MerchantInfo value object as owned entity
        builder.OwnsOne(s => s.MerchantInfo, merchantInfo =>
        {
            merchantInfo.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("MerchantEmail");

            merchantInfo.Property(m => m.Phone)
                .HasMaxLength(20)
                .HasColumnName("MerchantPhone");

            merchantInfo.Property(m => m.BusinessName)
                .HasMaxLength(200)
                .HasColumnName("MerchantBusinessName");

            merchantInfo.Property(m => m.Address)
                .HasMaxLength(500)
                .HasColumnName("MerchantAddress");

            merchantInfo.Property(m => m.City)
                .HasMaxLength(100)
                .HasColumnName("MerchantCity");

            merchantInfo.Property(m => m.State)
                .HasMaxLength(100)
                .HasColumnName("MerchantState");

            merchantInfo.Property(m => m.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("MerchantPostalCode");

            merchantInfo.Property(m => m.Country)
                .HasMaxLength(100)
                .HasColumnName("MerchantCountry");
        });

        // Configure StoreSettings value object as owned entity
        builder.OwnsOne(s => s.Settings, settings =>
        {
            settings.Property(st => st.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("Currency");

            settings.Property(st => st.Timezone)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Timezone");

            settings.Property(st => st.Language)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("Language");

            settings.Property(st => st.TaxRate)
                .IsRequired()
                .HasPrecision(5, 2)
                .HasColumnName("TaxRate");

            settings.Property(st => st.TaxEnabled)
                .IsRequired()
                .HasColumnName("TaxEnabled");

            settings.Property(st => st.PrimaryColor)
                .HasMaxLength(7)
                .HasColumnName("PrimaryColor");

            settings.Property(st => st.AccentColor)
                .HasMaxLength(7)
                .HasColumnName("AccentColor");

            settings.Property(st => st.LogoUrl)
                .HasMaxLength(2048)
                .HasColumnName("LogoUrl");
        });

        // Ignore computed properties
        builder.Ignore(s => s.IsInTrial);
        builder.Ignore(s => s.IsActive);
    }
}
