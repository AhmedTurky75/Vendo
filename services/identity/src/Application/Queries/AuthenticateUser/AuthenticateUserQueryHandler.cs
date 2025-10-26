using AutoMapper;
using MediatR;
using Vendo.IdentityManagement.Application.Common.Interfaces;
using Vendo.IdentityManagement.Application.Common.Models;
using Vendo.IdentityManagement.Application.DTOs;
using Vendo.IdentityManagement.Domain.Repositories;

namespace Vendo.IdentityManagement.Application.Queries.AuthenticateUser;

/// <summary>
/// Handler for AuthenticateUserQuery
/// </summary>
public class AuthenticateUserQueryHandler : IRequestHandler<AuthenticateUserQuery, Result<AuthenticationResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public AuthenticateUserQueryHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
    }

    public async Task<Result<AuthenticationResultDto>> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

            if (user == null)
            {
                return Result<AuthenticationResultDto>.Success(new AuthenticationResultDto
                {
                    IsAuthenticated = false,
                    Message = "Invalid username or password"
                });
            }

            if (!user.IsActive)
            {
                return Result<AuthenticationResultDto>.Success(new AuthenticationResultDto
                {
                    IsAuthenticated = false,
                    Message = "Account is deactivated"
                });
            }

            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Result<AuthenticationResultDto>.Success(new AuthenticationResultDto
                {
                    IsAuthenticated = false,
                    Message = "Invalid username or password"
                });
            }

            // If a specific role is requested, verify the user has that role
            if (!string.IsNullOrEmpty(request.Role))
            {
                if (!user.Roles.Any(r => r.Equals(request.Role, StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<AuthenticationResultDto>.Success(new AuthenticationResultDto
                    {
                        IsAuthenticated = false,
                        Message = $"User does not have the required role: {request.Role}"
                    });
                }
            }

            // Generate JWT tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresIn = _jwtTokenService.GetTokenExpirationInSeconds();

            var userDto = _mapper.Map<UserDto>(user);

            return Result<AuthenticationResultDto>.Success(new AuthenticationResultDto
            {
                IsAuthenticated = true,
                User = userDto,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                Message = "Authentication successful"
            });
        }
        catch (Exception ex)
        {
            return Result<AuthenticationResultDto>.Failure($"An error occurred during authentication: {ex.Message}");
        }
    }
}
