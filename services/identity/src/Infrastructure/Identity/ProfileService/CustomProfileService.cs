using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Infrastructure.Identity.ProfileService;

/// <summary>
/// Custom profile service for IdentityServer to include user claims
/// </summary>
public class CustomProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;

    public CustomProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subject = context.Subject;
        if (subject == null)
        {
            return;
        }

        var subjectId = subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId) || !Guid.TryParse(subjectId, out var userId))
        {
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("email", user.Email.Value),
            new Claim("username", user.Username)
        };

        // Add roles
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role));
        }

        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var subject = context.Subject;
        if (subject == null)
        {
            context.IsActive = false;
            return;
        }

        var subjectId = subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId) || !Guid.TryParse(subjectId, out var userId))
        {
            context.IsActive = false;
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        context.IsActive = user?.IsActive ?? false;
    }
}
