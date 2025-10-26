using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;

namespace Vendo.IdentityManagement.Application.Queries.AuthenticateUser;

/// <summary>
/// Query to authenticate a user
/// </summary>
public class AuthenticateUserQuery : IRequest<Result<AuthenticationResultDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Role { get; set; }
}
