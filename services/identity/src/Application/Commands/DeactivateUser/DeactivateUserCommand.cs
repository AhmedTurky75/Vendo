using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;

namespace Vendo.IdentityManagement.Application.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user account
/// </summary>
public class DeactivateUserCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
}
