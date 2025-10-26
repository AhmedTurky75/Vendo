using MediatR;
using Vendo.IdentityManagement.Application.Common.Interfaces;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Domain.Exceptions;
using Vendo.IdentityManagement.Domain.Repositories;
using Vendo.IdentityManagement.Domain.ValueObjects;

namespace Vendo.IdentityManagement.Application.Commands.ChangePassword;

/// <summary>
/// Handler for ChangePasswordCommand
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return Result.Failure("Current password is incorrect");
            }

            // Validate new password
            var newPassword = Password.Create(request.NewPassword);

            // Hash new password
            var newPasswordHash = _passwordHasher.HashPassword(newPassword.Value);

            // Update password
            user.ChangePassword(newPasswordHash);
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success();
        }
        catch (DomainValidationException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while changing password: {ex.Message}");
        }
    }
}
