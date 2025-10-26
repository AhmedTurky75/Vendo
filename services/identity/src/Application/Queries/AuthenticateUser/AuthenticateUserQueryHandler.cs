using MediatR;
using Microsoft.AspNetCore.Identity;
using Vendo.IdentityManagement.Application.Common.Interfaces;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Application.Queries.AuthenticateUser;

/// <summary>
/// Handler for AuthenticateUserQuery
/// </summary>
public class AuthenticateUserQueryHandler : IRequestHandler<AuthenticateUserQuery, Result<AuthenticationResultDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticateUserQueryHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthenticationResultDto>> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            
            if (user == null)
            {
                return Result<AuthenticationResultDto>.Failure("Invalid username or password");
            }

            if (!user.IsActive)
            {
                return Result<AuthenticationResultDto>.Failure("User account is not active");
            }

            // Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return Result<AuthenticationResultDto>.Failure("Invalid username or password");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user.Id, user.UserName!, roles.ToList());

            var authResult = new AuthenticationResultDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            };

            return Result<AuthenticationResultDto>.Success(authResult);
        }
        catch (Exception ex)
        {
            return Result<AuthenticationResultDto>.Failure($"An error occurred during authentication: {ex.Message}");
        }
    }
}
