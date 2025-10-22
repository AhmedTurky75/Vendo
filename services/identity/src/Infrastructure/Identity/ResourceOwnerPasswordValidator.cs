using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Vendo.Identity.Application.Common.Interfaces;
using Vendo.Identity.Domain.Repositories;

namespace Vendo.Identity.Infrastructure.Identity;

/// <summary>
/// Validates resource owner password credentials
/// </summary>
public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResourceOwnerPasswordValidator(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        var user = await _userRepository.GetByUsernameAsync(context.UserName);

        if (user == null)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Invalid username or password");
            return;
        }

        if (!user.IsActive)
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Account is deactivated");
            return;
        }

        if (!_passwordHasher.VerifyPassword(context.Password, user.PasswordHash))
        {
            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Invalid username or password");
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("name", $"{user.FirstName} {user.LastName}"),
            new Claim("email", user.Email.Value),
            new Claim("username", user.Username)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim("role", role));
        }

        context.Result = new GrantValidationResult(
            subject: user.Id.ToString(),
            authenticationMethod: "password",
            claims: claims);
    }
}
