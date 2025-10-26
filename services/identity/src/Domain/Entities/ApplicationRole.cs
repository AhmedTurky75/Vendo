using Microsoft.AspNetCore.Identity;

namespace Vendo.IdentityManagement.Domain.Entities;

/// <summary>
/// Application role entity extending ASP.NET Identity's IdentityRole with custom fields.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Description of the role's purpose and responsibilities.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of permissions associated with this role.
    /// Stored as JSON array of permission strings (e.g., ["products.create", "orders.view"]).
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Indicates whether this is a system-defined role that cannot be deleted.
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Timestamp when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
