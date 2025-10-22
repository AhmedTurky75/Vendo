namespace Vendo.Identity.Application.DTOs;

/// <summary>
/// Data transfer object for authentication result
/// </summary>
public class AuthenticationResultDto
{
    public bool IsAuthenticated { get; set; }
    public UserDto? User { get; set; }
    public string? Message { get; set; }
}
