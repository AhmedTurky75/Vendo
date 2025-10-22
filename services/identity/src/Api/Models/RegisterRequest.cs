namespace Vendo.Identity.Api.Models;

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Username for the new user
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address for the new user
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for the new user
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// First name of the user
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the user
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
