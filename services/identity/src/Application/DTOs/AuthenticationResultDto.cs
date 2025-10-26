namespace Vendo.IdentityManagement.Application.DTOs;

/// <summary>
/// Data transfer object for authentication result
/// </summary>
public class AuthenticationResultDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
