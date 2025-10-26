using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Vendo.IdentityManagement.Domain.Entities;

namespace Vendo.IdentityManagement.Infrastructure.Identity.ProfileService;

/// <summary>
/// Custom profile service for IdentityServer to include user claims
/// Includes tenant information and role permissions for multi-tenancy support
/// </summary>
public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<CustomProfileService> _logger;

    public CustomProfileService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<CustomProfileService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
        if (string.IsNullOrEmpty(subjectId))
        {
            _logger.LogWarning("Profile data requested with invalid subject ID: {SubjectId}", subjectId);
            return;
        }

        var user = await _userManager.FindByIdAsync(subjectId);
        if (user == null)
        {
            _logger.LogWarning("User not found for profile data request: {UserId}", subjectId);
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("email_verified", user.EmailConfirmed.ToString().ToLower()),
            new Claim("username", user.UserName ?? string.Empty),
            new Claim("updated_at", user.UpdatedAt.ToString("O"))
        };

        // Add phone number if available
        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            claims.Add(new Claim("phone_number", user.PhoneNumber));
            claims.Add(new Claim("phone_number_verified", user.PhoneNumberConfirmed.ToString().ToLower()));
        }

        // Add roles - IdentityServer requires proper role claim type
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));

            // Add permissions for this role
            var roleEntity = await _roleManager.FindByNameAsync(role);
            if (roleEntity != null && roleEntity.Permissions.Any())
            {
                foreach (var permission in roleEntity.Permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }
        }

        // Add store-specific claims for merchant users
        if (user.StoreId.HasValue)
        {
            claims.Add(new Claim("store_id", user.StoreId.Value.ToString()));
        }

        // Add tenant claims for multi-tenancy support
        // For now using a default tenant - can be extended based on user properties
        if (context.RequestedClaimTypes.Contains("tenant_id") ||
            context.RequestedResources?.ParsedScopes?.Any(s => s.ParsedName == "tenant") == true)
        {
            claims.Add(new Claim("tenant_id", "default"));
            claims.Add(new Claim("tenant_name", "Default Tenant"));
        }

        // Log successful profile data retrieval
        _logger.LogInformation("Profile data retrieved for user {UserId} with {ClaimCount} claims",
            user.Id, claims.Count);

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
        if (string.IsNullOrEmpty(subjectId))
        {
            _logger.LogWarning("IsActive check requested with invalid subject ID: {SubjectId}", subjectId);
            context.IsActive = false;
            return;
        }

        var user = await _userManager.FindByIdAsync(subjectId);
        var isActive = user?.IsActive ?? false;

        _logger.LogInformation("IsActive check for user {UserId}: {IsActive}", subjectId, isActive);

        context.IsActive = isActive;
    }
}
