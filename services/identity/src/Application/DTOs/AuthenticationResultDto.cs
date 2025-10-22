namespace Vendo.Identity.Application.DTOs;

/// <summary>
/// Data transfer object for authentication result
/// </summary>
public class AuthenticationResultDto
{
    public bool IsAuthenticated { get; set; }
    public UserDto? User { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int? ExpiresIn { get; set; }
    public string? Message { get; set; }
}
