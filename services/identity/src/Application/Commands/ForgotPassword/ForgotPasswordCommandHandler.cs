using MediatR;
using Microsoft.AspNetCore.Identity;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Application.Commands.ForgotPassword;

/// <summary>
/// Handler for ForgotPasswordCommand
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                // Don't reveal that the user doesn't exist for security reasons
                return Result<string>.Failure("If the email exists, a password reset token will be generated");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // In a real application, you would send this token via email
            // For now, we'll just return it (this is NOT production-ready)
            return Result<string>.Success(token);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"An error occurred while processing forgot password request: {ex.Message}");
        }
    }
}
