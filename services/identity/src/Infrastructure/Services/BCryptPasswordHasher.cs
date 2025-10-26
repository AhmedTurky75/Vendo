using Vendo.IdentityManagement.Application.Common.Interfaces;
using BCrypt.Net;

namespace Vendo.IdentityManagement.Infrastructure.Services;

/// <summary>
/// BCrypt implementation of IPasswordHasher
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
