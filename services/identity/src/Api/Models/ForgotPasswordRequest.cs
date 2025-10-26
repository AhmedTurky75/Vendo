namespace Vendo.IdentityManagement.Api.Models;

/// <summary>
/// Request model for forgot password
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
