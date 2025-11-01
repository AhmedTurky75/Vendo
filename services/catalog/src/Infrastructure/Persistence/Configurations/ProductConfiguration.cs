using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.CatalogManagement.Domain.Entities;
using Vendo.CatalogManagement.Domain.Enums;
using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for Product aggregate root.
/// Maps value objects to database columns.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        // Configure SKU value object
        builder.OwnsOne(p => p.SKU, sku =>
        {
            sku.Property(s => s.Value)
                .HasColumnName("SKU")
                .IsRequired()
                .HasMaxLength(50);
        });

        // Configure Slug value object
        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("Slug")
                .IsRequired()
                .HasMaxLength(250);
        });

        // Configure Money value objects
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(p => p.CompareAtPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CompareAtPrice")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("CompareAtCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.CostPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CostPrice")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("CostCurrency")
                .HasMaxLength(3);
        });

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.LowStockThreshold)
            .IsRequired();

        builder.Property(p => p.TrackInventory)
            .IsRequired();

        builder.Property(p => p.IsTaxable)
            .IsRequired();

        builder.Property(p => p.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(p => p.Weight)
            .HasPrecision(10, 2);

        // Configure Dimensions value object
        builder.OwnsOne(p => p.Dimensions, dimensions =>
        {
            dimensions.Property(d => d.Length)
                .HasColumnName("DimensionLength")
                .HasPrecision(10, 2);

            dimensions.Property(d => d.Width)
                .HasColumnName("DimensionWidth")
                .HasPrecision(10, 2);

            dimensions.Property(d => d.Height)
                .HasColumnName("DimensionHeight")
                .HasPrecision(10, 2);

            dimensions.Property(d => d.Unit)
                .HasColumnName("DimensionUnit")
                .HasMaxLength(10);
        });

        // Configure ProductImages value object
        builder.OwnsOne(p => p.Images, images =>
        {
            images.Property(i => i.MainImageUrl)
                .HasColumnName("ImageUrl")
                .HasMaxLength(500);

            images.Property(i => i.AdditionalImageUrls)
                .HasColumnName("AdditionalImages")
                .HasMaxLength(2000)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        // Configure SEOMetadata value object
        builder.OwnsOne(p => p.SEOMetadata, seo =>
        {
            seo.Property(s => s.MetaTitle)
                .HasColumnName("MetaTitle")
                .HasMaxLength(200);

            seo.Property(s => s.MetaDescription)
                .HasColumnName("MetaDescription")
                .HasMaxLength(500);

            seo.Property(s => s.MetaKeywords)
                .HasColumnName("MetaKeywords")
                .HasMaxLength(500);
        });

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.IsFeatured)
            .IsRequired();

        // Store tags as JSON or comma-separated string
        builder.Property(p => p.Tags)
            .HasColumnName("Tags")
            .HasMaxLength(1000)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

        builder.Property(p => p.DisplayOrder)
            .IsRequired();

        builder.Property(p => p.ViewCount)
            .IsRequired();

        builder.Property(p => p.SalesCount)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Ignore domain events (not persisted)
        builder.Ignore(p => p.DomainEvents);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => new { p.TenantId })
            .HasDatabaseName("IX_Products_TenantId");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Products_Status");

        builder.HasIndex(p => p.IsFeatured)
            .HasDatabaseName("IX_Products_IsFeatured");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Products_CreatedAt");
    }
}
