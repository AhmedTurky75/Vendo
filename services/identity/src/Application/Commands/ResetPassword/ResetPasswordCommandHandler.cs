using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Domain.Exceptions;
using Vendo.Identity.Domain.Repositories;
using Vendo.Identity.Domain.ValueObjects;

namespace Vendo.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Handler for ResetPasswordCommand
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create and validate email
            var email = Email.Create(request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                return Result.Failure("Invalid or expired reset token");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Password reset attempted for inactive user: {UserId}", user.Id);
                return Result.Failure("Invalid or expired reset token");
            }

            // Validate and reset password with token
            try
            {
                // Validate new password
                var password = Password.Create(request.NewPassword);

                // Hash the new password
                var newPasswordHash = _passwordHasher.HashPassword(password.Value);

                // Reset password using the token
                user.ResetPasswordWithToken(request.Token, newPasswordHash);

                // Save the updated user
                await _userRepository.UpdateAsync(user, cancellationToken);

                _logger.LogInformation("Password successfully reset for user: {UserId}", user.Id);

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Password reset failed for user {UserId}: {Error}", user.Id, ex.Message);
                return Result.Failure("Invalid or expired reset token");
            }
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning("Password reset validation failed: {Error}", ex.Message);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset for email: {Email}", request.Email);
            return Result.Failure("An error occurred while resetting password");
        }
    }
}
