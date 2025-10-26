using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;

namespace Vendo.IdentityManagement.Application.Commands.ChangePassword;

/// <summary>
/// Command to change user password
/// </summary>
public class ChangePasswordCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
