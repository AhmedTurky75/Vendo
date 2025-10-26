using System.Security.Cryptography;
using Vendo.IdentityManagement.Domain.Exceptions;

namespace Vendo.IdentityManagement.Domain.ValueObjects;

/// <summary>
/// Value object for password reset token
/// </summary>
public class PasswordResetToken
{
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private PasswordResetToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Creates a new password reset token with a specified expiration time
    /// </summary>
    /// <param name="expirationMinutes">Token expiration time in minutes (default: 60)</param>
    /// <returns>New password reset token</returns>
    public static PasswordResetToken Create(int expirationMinutes = 60)
    {
        if (expirationMinutes <= 0)
        {
            throw new DomainValidationException("Token expiration must be greater than 0");
        }

        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return new PasswordResetToken(token, expiresAt);
    }

    /// <summary>
    /// Creates a password reset token from existing values (for reconstitution from storage)
    /// </summary>
    public static PasswordResetToken FromExisting(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainValidationException("Token cannot be empty");
        }

        return new PasswordResetToken(token, expiresAt);
    }

    /// <summary>
    /// Checks if the token is still valid (not expired)
    /// </summary>
    public bool IsValid()
    {
        return DateTime.UtcNow < ExpiresAt;
    }

    /// <summary>
    /// Validates if the provided token matches this token and is not expired
    /// </summary>
    public bool Matches(string providedToken)
    {
        if (string.IsNullOrWhiteSpace(providedToken))
        {
            return false;
        }

        return Token == providedToken && IsValid();
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PasswordResetToken other)
        {
            return false;
        }

        return Token == other.Token && ExpiresAt == other.ExpiresAt;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Token, ExpiresAt);
    }
}
