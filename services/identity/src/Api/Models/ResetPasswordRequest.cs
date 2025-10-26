namespace Vendo.IdentityManagement.Api.Models;

/// <summary>
/// Request model for reset password
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password reset token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}
