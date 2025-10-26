using Microsoft.AspNetCore.Identity;

namespace Vendo.IdentityManagement.Domain.Entities;

/// <summary>
/// Application user entity extending ASP.NET Identity's IdentityUser with custom fields.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// URL to the user's profile picture.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// User's date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Store ID for merchant users (null for regular customers).
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// User's address stored as JSON string.
    /// Contains: addressLine1, addressLine2, city, state, postalCode, country
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// User preferences stored as JSON string.
    /// Contains: newsletter, smsNotifications, emailNotifications, language, timezone
    /// </summary>
    public string? Preferences { get; set; }

    /// <summary>
    /// Timestamp of the user's last login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user account was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
