using AutoMapper;
using MediatR;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;
using Vendo.Identity.Domain.Entities;
using Vendo.Identity.Domain.Exceptions;
using Vendo.Identity.Domain.Repositories;
using Vendo.Identity.Domain.ValueObjects;

namespace Vendo.Identity.Application.Commands.RegisterUser;

/// <summary>
/// Handler for RegisterUserCommand
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
            {
                return Result<UserDto>.Failure($"Username '{request.Username}' is already taken");
            }

            // Create and validate email
            var email = Email.Create(request.Email);

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(email, cancellationToken))
            {
                return Result<UserDto>.Failure($"Email '{request.Email}' is already registered");
            }

            // Validate password
            var password = Password.Create(request.Password);

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(password.Value);

            // Create user entity
            var user = User.Create(
                request.Username,
                email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.Roles ?? new List<string> { "User" });

            // Save to repository
            await _userRepository.AddAsync(user, cancellationToken);

            // Map to DTO and return
            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (DomainValidationException ex)
        {
            return Result<UserDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while registering user: {ex.Message}");
        }
    }
}
