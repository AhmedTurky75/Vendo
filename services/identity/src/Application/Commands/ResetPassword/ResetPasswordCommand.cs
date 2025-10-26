using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;

namespace Vendo.IdentityManagement.Application.Commands.ResetPassword;

/// <summary>
/// Command to reset user password with token
/// </summary>
public class ResetPasswordCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
