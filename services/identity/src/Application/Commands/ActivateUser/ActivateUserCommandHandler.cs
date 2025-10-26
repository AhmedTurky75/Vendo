using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Domain.Repositories;

namespace Vendo.IdentityManagement.Application.Commands.ActivateUser;

/// <summary>
/// Handler for ActivateUserCommand
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public ActivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.Activate();
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while activating user: {ex.Message}");
        }
    }
}
