namespace Vendo.Identity.Application.Common.Interfaces;

/// <summary>
/// Service interface for password hashing operations
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
