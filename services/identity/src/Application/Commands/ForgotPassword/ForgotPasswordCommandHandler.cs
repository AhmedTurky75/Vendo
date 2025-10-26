using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Domain.Repositories;
using Vendo.IdentityManagement.Domain.ValueObjects;

namespace Vendo.IdentityManagement.Application.Commands.ForgotPassword;

/// <summary>
/// Handler for ForgotPasswordCommand
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create and validate email
            var email = Email.Create(request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            // Always return success to prevent email enumeration attacks
            // Don't reveal whether the email exists or not
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Password reset requested for non-existent or inactive email: {Email}", request.Email);
                return Result.Success();
            }

            // Generate password reset token (expires in 60 minutes)
            var resetToken = PasswordResetToken.Create(expirationMinutes: 60);
            user.SetPasswordResetToken(resetToken);

            // Save the user with the reset token
            await _userRepository.UpdateAsync(user, cancellationToken);

            // In development: log the token to console
            // TODO: In production, send email with reset link containing the token
            _logger.LogInformation(
                "Password reset token generated for user {UserId}. Token: {Token}, Expires at: {ExpiresAt}",
                user.Id,
                resetToken.Token,
                resetToken.ExpiresAt);

            _logger.LogInformation(
                "DEV MODE - Password Reset URL: http://localhost:4200/reset-password?token={Token}&email={Email}",
                resetToken.Token,
                request.Email);

            // TODO: Production - Send email with reset link
            // await _emailService.SendPasswordResetEmailAsync(user.Email.Value, resetToken.Token);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
            // Still return success to prevent information disclosure
            return Result.Success();
        }
    }
}
