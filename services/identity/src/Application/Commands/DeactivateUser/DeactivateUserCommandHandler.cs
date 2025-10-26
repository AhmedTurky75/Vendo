using MediatR;
using Microsoft.AspNetCore.Identity;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Application.Commands.DeactivateUser;

/// <summary>
/// Handler for DeactivateUserCommand
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeactivateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            
            if (user == null)
            {
                return Result<bool>.Failure($"User with ID '{request.UserId}' not found");
            }

            if (!user.IsActive)
            {
                return Result<bool>.Failure("User is already inactive");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure($"Failed to deactivate user: {errors}");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"An error occurred while deactivating user: {ex.Message}");
        }
    }
}
