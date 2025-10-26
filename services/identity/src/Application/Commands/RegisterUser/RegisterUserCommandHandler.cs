using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Application.Commands.RegisterUser;

/// <summary>
/// Handler for RegisterUserCommand
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);
            if (existingUserByUsername != null)
            {
                return Result<UserDto>.Failure($"Username '{request.Username}' is already taken");
            }

            // Check if email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return Result<UserDto>.Failure($"Email '{request.Email}' is already registered");
            }

            // Create user entity
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"Failed to create user: {errors}");
            }

            // Assign roles
            var roles = request.Roles ?? new List<string> { "Customer" };
            var roleResult = await _userManager.AddToRolesAsync(user, roles);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"User created but failed to assign roles: {errors}");
            }

            // Map to DTO and return
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while registering user: {ex.Message}");
        }
    }
}
