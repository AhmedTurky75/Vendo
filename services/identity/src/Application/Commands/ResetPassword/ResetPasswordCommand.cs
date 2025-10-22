using MediatR;
using Vendo.Identity.Application.Common.Models;

namespace Vendo.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Command to reset user password with token
/// </summary>
public class ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
