using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vendo.IdentityManagement.Application.Common.Interfaces;

namespace Vendo.IdentityManagement.Infrastructure.Services;

/// <summary>
/// JWT token service implementation
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly int _tokenExpirationMinutes;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        // Read configuration with fallback defaults for development
        _secret = _configuration["Jwt:Secret"] ?? "VendoIdentitySecretKeyForDevelopmentMinimum32Characters!";
        _issuer = _configuration["Jwt:Issuer"] ?? "https://localhost:5001";
        _audience = _configuration["Jwt:Audience"] ?? "vendo.api";

        if (!int.TryParse(_configuration["Jwt:ExpirationMinutes"], out _tokenExpirationMinutes))
        {
            _tokenExpirationMinutes = 60; // Default to 60 minutes
        }
    }

    public string GenerateToken(Guid userId, string username, List<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public int GetTokenExpirationInSeconds()
    {
        return _tokenExpirationMinutes * 60;
    }
}
