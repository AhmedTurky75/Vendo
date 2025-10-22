using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.Logging;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Infrastructure.Identity.ProfileService;

/// <summary>
/// Custom profile service for IdentityServer to include user claims
/// Includes tenant information for multi-tenancy support
/// </summary>
public class CustomProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CustomProfileService> _logger;

    public CustomProfileService(IUserRepository userRepository, ILogger<CustomProfileService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subject = context.Subject;
        if (subject == null)
        {
            _logger.LogWarning("Profile data requested with null subject");
            return;
        }

        var subjectId = subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId) || !Guid.TryParse(subjectId, out var userId))
        {
            _logger.LogWarning("Profile data requested with invalid subject ID: {SubjectId}", subjectId);
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile data request: {UserId}", userId);
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("email", user.Email.Value),
            new Claim("email_verified", "true"), // Assuming email is verified
            new Claim("username", user.Username),
            new Claim("updated_at", user.UpdatedAt.ToString("O"))
        };

        // Add roles - IdentityServer requires proper role claim type
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role));
        }

        // Add tenant claims for multi-tenancy support
        // TODO: Once User entity has TenantId, retrieve from user object
        // For now, add placeholder tenant claim if tenant scope is requested
        if (context.RequestedClaimTypes.Contains("tenant_id") ||
            context.RequestedResources?.ParsedScopes?.Any(s => s.ParsedName == "tenant") == true)
        {
            // Default tenant for MVP - will be replaced with actual tenant from User entity
            claims.Add(new Claim("tenant_id", "default"));
            claims.Add(new Claim("tenant_name", "Default Tenant"));
        }

        // Log successful profile data retrieval
        _logger.LogInformation("Profile data retrieved for user {UserId} with {ClaimCount} claims",
            userId, claims.Count);

        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var subject = context.Subject;
        if (subject == null)
        {
            _logger.LogWarning("IsActive check requested with null subject");
            context.IsActive = false;
            return;
        }

        var subjectId = subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId) || !Guid.TryParse(subjectId, out var userId))
        {
            _logger.LogWarning("IsActive check requested with invalid subject ID: {SubjectId}", subjectId);
            context.IsActive = false;
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        var isActive = user?.IsActive ?? false;

        _logger.LogInformation("IsActive check for user {UserId}: {IsActive}", userId, isActive);

        context.IsActive = isActive;
    }
}
