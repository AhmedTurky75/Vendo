namespace Vendo.IdentityManagement.Application.Common.Interfaces;

/// <summary>
/// Service interface for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="username">The username</param>
    /// <param name="roles">The user's roles</param>
    /// <returns>JWT access token</returns>
    string GenerateToken(Guid userId, string username, List<string> roles);

    /// <summary>
    /// Gets the token expiration time in seconds
    /// </summary>
    int GetTokenExpirationInSeconds();
}
