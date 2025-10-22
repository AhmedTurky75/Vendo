using AutoMapper;
using MediatR;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Application.Common.Models;
using Vendo.Identity.Application.DTOs;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Application.Queries.AuthenticateUser;

/// <summary>
/// Handler for AuthenticateUserQuery
/// </summary>
public class AuthenticateUserQueryHandler : IRequestHandler<AuthenticateUserQuery, Result<AuthenticationResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public AuthenticateUserQueryHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
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

            var userDto = _mapper.Map<UserDto>(user);

            return Result<AuthenticationResultDto>.Success(new AuthenticationResultDto
            {
                IsAuthenticated = true,
                User = userDto,
                Message = "Authentication successful"
            });
        }
        catch (Exception ex)
        {
            return Result<AuthenticationResultDto>.Failure($"An error occurred during authentication: {ex.Message}");
        }
    }
}
