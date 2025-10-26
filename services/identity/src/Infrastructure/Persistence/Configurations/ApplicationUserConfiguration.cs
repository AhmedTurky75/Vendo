using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ApplicationUser entity.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Primary key is already configured by IdentityDbContext

        // Required fields
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Optional fields with max lengths
        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(500);

        // JSON columns for complex objects
        builder.Property(u => u.Address)
            .HasColumnType("nvarchar(max)");

        builder.Property(u => u.Preferences)
            .HasColumnType("nvarchar(max)");

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");

        builder.HasIndex(u => u.StoreId)
            .HasDatabaseName("IX_Users_StoreId");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");
    }
}
