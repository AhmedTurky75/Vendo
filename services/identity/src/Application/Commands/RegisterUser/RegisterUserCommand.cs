using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;

namespace Vendo.IdentityManagement.Application.Commands.RegisterUser;

/// <summary>
/// Command to register a new user
/// </summary>
public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
}
