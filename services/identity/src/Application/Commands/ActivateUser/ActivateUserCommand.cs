using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;

namespace Vendo.IdentityManagement.Application.Commands.ActivateUser;

/// <summary>
/// Command to activate a user account
/// </summary>
public class ActivateUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}
