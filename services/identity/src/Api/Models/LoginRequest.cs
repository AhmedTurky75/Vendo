namespace Vendo.IdentityManagement.Api.Models;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username or email
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Role to authenticate as (Admin, Merchant, Customer) - optional for role-specific login
    /// </summary>
    public string? Role { get; set; }
}
