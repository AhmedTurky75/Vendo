using MediatR;
using Vendo.Identity.Application.Common.Models;

namespace Vendo.Identity.Application.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process
/// </summary>
public class ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}
