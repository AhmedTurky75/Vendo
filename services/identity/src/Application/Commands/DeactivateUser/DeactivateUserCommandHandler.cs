using MediatR;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Application.Commands.DeactivateUser;

/// <summary>
/// Handler for DeactivateUserCommand
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public DeactivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            user.Deactivate();
            await _userRepository.UpdateAsync(user, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while deactivating user: {ex.Message}");
        }
    }
}
