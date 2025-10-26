using AutoMapper;
using MediatR;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;
using Vendo.IdentityManagement.Domain.Exceptions;
using Vendo.IdentityManagement.Domain.Repositories;
using Vendo.IdentityManagement.Domain.ValueObjects;

namespace Vendo.IdentityManagement.Application.Commands.UpdateUser;

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var email = Email.Create(request.Email);

            // Check if email is being changed and if it's already taken
            if (user.Email.Value != email.Value)
            {
                if (await _userRepository.EmailExistsAsync(email, cancellationToken))
                {
                    return Result<UserDto>.Failure($"Email '{request.Email}' is already registered");
                }
            }

            user.UpdateProfile(request.FirstName, request.LastName, email);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (DomainValidationException ex)
        {
            return Result<UserDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while updating user: {ex.Message}");
        }
    }
}
