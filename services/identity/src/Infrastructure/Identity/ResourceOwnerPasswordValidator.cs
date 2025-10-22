using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Infrastructure.Identity;

/// <summary>
/// Validates resource owner password credentials for OAuth2 Resource Owner Password flow
/// This flow should be phased out in favor of Authorization Code + PKCE for better security
/// </summary>
public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResourceOwnerPasswordValidator> _logger;

    public ResourceOwnerPasswordValidator(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<ResourceOwnerPasswordValidator> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        _logger.LogInformation("Resource Owner Password validation started for username: {Username}", context.UserName);

        var user = await _userRepository.GetByUsernameAsync(context.UserName);

        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User not found - {Username}", context.UserName);
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Invalid username or password");
            return;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Authentication failed: Account deactivated - UserId: {UserId}, Username: {Username}",
                user.Id, context.UserName);
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Account is deactivated");
            return;
        }

        if (!_passwordHasher.VerifyPassword(context.Password, user.PasswordHash))
        {
            _logger.LogWarning("Authentication failed: Invalid password - UserId: {UserId}, Username: {Username}",
                user.Id, context.UserName);
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Invalid username or password");
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("email", user.Email.Value),
            new Claim("email_verified", "true"),
            new Claim("username", user.Username),
            new Claim("updated_at", user.UpdatedAt.ToString("O"))
        };

        // Add all user roles
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role));
        }

        // Add tenant claims for multi-tenancy support
        // TODO: Once User entity has TenantId, retrieve from user object
        // For now, add placeholder tenant claim
        claims.Add(new Claim("tenant_id", "default"));
        claims.Add(new Claim("tenant_name", "Default Tenant"));

        _logger.LogInformation("Authentication successful for UserId: {UserId}, Username: {Username}, Roles: {Roles}",
            user.Id, context.UserName, string.Join(", ", user.Roles));

        context.Result = new GrantValidationResult(
            subject: user.Id.ToString(),
            authenticationMethod: "password",
            claims: claims);
    }
}
