using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;

namespace Vendo.Identity.Application.Commands.UpdateUser;

/// <summary>
/// Command to update user profile
/// </summary>
public class UpdateUserCommand : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
