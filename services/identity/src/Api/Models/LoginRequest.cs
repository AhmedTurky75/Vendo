namespace Vendo.Identity.Api.Models;

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
}
