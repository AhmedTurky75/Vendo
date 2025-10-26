using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;

namespace Vendo.IdentityManagement.Application.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process
/// </summary>
public class ForgotPasswordCommand : IRequest<Result<string>>
{
    public string Email { get; set; } = string.Empty;
}
