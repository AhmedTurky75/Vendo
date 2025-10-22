using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;

namespace Vendo.Identity.Application.Queries.AuthenticateUser;

/// <summary>
/// Query to authenticate a user
/// </summary>
public class AuthenticateUserQuery : IRequest<Result<AuthenticationResultDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
