using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Vendo.Identity.Infrastructure.Identity.Configuration;

/// <summary>
/// Configuration for Duende IdentityServer resources and clients
/// </summary>
public static class IdentityServerConfig
{
    /// <summary>
    /// Define API resources
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("vendo.api", "Vendo API")
            {
                Scopes = { "vendo.api.full_access", "vendo.api.read", "vendo.api.write" }
            }
        };

    /// <summary>
    /// Define API scopes
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("vendo.api.full_access", "Full access to Vendo API"),
            new ApiScope("vendo.api.read", "Read access to Vendo API"),
            new ApiScope("vendo.api.write", "Write access to Vendo API")
        };

    /// <summary>
    /// Define identity resources (user claims)
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("roles", "User roles", new[] { "role" })
        };

    /// <summary>
    /// Define clients (applications that can request tokens)
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // Swagger UI client
            new Client
            {
                ClientId = "swagger",
                ClientName = "Swagger UI",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "https://localhost:5001/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { "https://localhost:5001" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles"
                }
            },
            // Resource Owner Password Credentials client (for testing)
            new Client
            {
                ClientId = "client",
                ClientName = "Client Application",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles"
                }
            },
            // Interactive application (for testing)
            new Client
            {
                ClientId = "interactive",
                ClientName = "Interactive Application",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RedirectUris = { "https://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles"
                }
            }
        };
}
