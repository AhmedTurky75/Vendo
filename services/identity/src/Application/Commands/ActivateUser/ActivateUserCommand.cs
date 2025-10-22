using MediatR;
using Vendo.Identity.Application.Common.Models;

namespace Vendo.Identity.Application.Commands.ActivateUser;

/// <summary>
/// Command to activate a user account
/// </summary>
public class ActivateUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}
