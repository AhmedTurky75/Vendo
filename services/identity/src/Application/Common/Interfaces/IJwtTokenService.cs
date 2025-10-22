using Vendo.Identity.Domain.Entities;

namespace Vendo.Identity.Application.Common.Interfaces;

/// <summary>
/// Service interface for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    /// <param name="user">The user to generate token for</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token for a user
    /// </summary>
    /// <returns>Refresh token</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Gets the token expiration time in seconds
    /// </summary>
    int GetTokenExpirationInSeconds();
}
