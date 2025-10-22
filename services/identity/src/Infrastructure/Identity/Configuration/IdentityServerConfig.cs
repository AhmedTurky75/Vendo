using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Vendo.Identity.Infrastructure.Identity.Configuration;

/// <summary>
/// Configuration for Duende IdentityServer resources and clients
/// Implements proper OAuth2/OIDC flows per security requirements
/// </summary>
public static class IdentityServerConfig
{
    /// <summary>
    /// Define API resources with proper scopes
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("vendo.api", "Vendo API")
            {
                Scopes = { "vendo.api.full_access", "vendo.api.read", "vendo.api.write" },
                UserClaims = { "role", "email", "name", "username", "tenant_id" }
            }
        };

    /// <summary>
    /// Define API scopes for fine-grained access control
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("vendo.api.full_access", "Full access to Vendo API")
            {
                UserClaims = { "role", "email", "name", "username", "tenant_id" }
            },
            new ApiScope("vendo.api.read", "Read access to Vendo API")
            {
                UserClaims = { "role", "email", "name", "username", "tenant_id" }
            },
            new ApiScope("vendo.api.write", "Write access to Vendo API")
            {
                UserClaims = { "role", "email", "name", "username", "tenant_id" }
            }
        };

    /// <summary>
    /// Define identity resources (user claims) including tenant information
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("roles", "User roles", new[] { "role" }),
            new IdentityResource("tenant", "Tenant information", new[] { "tenant_id", "tenant_name" })
        };

    /// <summary>
    /// Define clients (applications that can request tokens)
    /// Supports multiple OAuth2/OIDC flows per security requirements
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // Swagger UI client - Authorization Code with PKCE
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
                    "roles",
                    "tenant"
                },
                AccessTokenLifetime = 3600, // 1 hour
                AllowOfflineAccess = false
            },

            // Interactive web application - Authorization Code with PKCE (Recommended)
            new Client
            {
                ClientId = "interactive",
                ClientName = "Interactive Web Application",
                ClientSecrets = { new Secret("secret".Sha256()) }, // TODO: Move to secure configuration
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RedirectUris = {
                    "https://localhost:5002/signin-oidc",
                    "https://localhost:4200/auth/callback" // Angular/React SPA
                },
                PostLogoutRedirectUris = {
                    "https://localhost:5002/signout-callback-oidc",
                    "https://localhost:4200/auth/signout-callback"
                },
                AllowedCorsOrigins = {
                    "https://localhost:5002",
                    "https://localhost:4200"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles",
                    "tenant"
                },
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 1296000, // 15 days
                AllowOfflineAccess = true
            },

            // SPA Client - Authorization Code with PKCE (No client secret)
            new Client
            {
                ClientId = "spa",
                ClientName = "Single Page Application",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = {
                    "https://localhost:4200/auth/callback",
                    "http://localhost:4200/auth/callback" // For development
                },
                PostLogoutRedirectUris = {
                    "https://localhost:4200/auth/signout-callback",
                    "http://localhost:4200/auth/signout-callback"
                },
                AllowedCorsOrigins = {
                    "https://localhost:4200",
                    "http://localhost:4200"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "vendo.api.read",
                    "vendo.api.write",
                    "roles",
                    "tenant"
                },
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 604800, // 7 days (shorter for SPAs)
                AllowOfflineAccess = true
            },

            // Mobile Application Client - Authorization Code with PKCE
            new Client
            {
                ClientId = "mobile",
                ClientName = "Mobile Application",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = {
                    "com.vendo.app://callback",
                    "vendo://callback"
                },
                PostLogoutRedirectUris = {
                    "com.vendo.app://signout-callback",
                    "vendo://signout-callback"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles",
                    "tenant"
                },
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 2592000, // 30 days
                AllowOfflineAccess = true
            },

            // Service-to-Service Client - Client Credentials Flow
            new Client
            {
                ClientId = "service",
                ClientName = "Backend Service",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("service-secret".Sha256()) // TODO: Move to secure configuration
                },
                AllowedScopes =
                {
                    "vendo.api.full_access",
                    "vendo.api.read",
                    "vendo.api.write"
                },
                AccessTokenLifetime = 3600, // 1 hour
                Claims = new List<ClientClaim>
                {
                    new ClientClaim("role", "Service"),
                    new ClientClaim("client_type", "service")
                }
            },

            // Admin Portal Client - Authorization Code with PKCE
            new Client
            {
                ClientId = "admin-portal",
                ClientName = "Admin Portal",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = {
                    "https://localhost:4300/auth/callback",
                    "http://localhost:4300/auth/callback"
                },
                PostLogoutRedirectUris = {
                    "https://localhost:4300/auth/signout-callback",
                    "http://localhost:4300/auth/signout-callback"
                },
                AllowedCorsOrigins = {
                    "https://localhost:4300",
                    "http://localhost:4300"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles",
                    "tenant"
                },
                AccessTokenLifetime = 1800, // 30 minutes (shorter for admin)
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 43200, // 12 hours (shorter for admin)
                AllowOfflineAccess = true,
                RequireConsent = false
            },

            // Merchant Portal Client - Authorization Code with PKCE
            new Client
            {
                ClientId = "merchant-portal",
                ClientName = "Merchant Portal",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = {
                    "https://localhost:4400/auth/callback",
                    "http://localhost:4400/auth/callback"
                },
                PostLogoutRedirectUris = {
                    "https://localhost:4400/auth/signout-callback",
                    "http://localhost:4400/auth/signout-callback"
                },
                AllowedCorsOrigins = {
                    "https://localhost:4400",
                    "http://localhost:4400"
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.read",
                    "vendo.api.write",
                    "roles",
                    "tenant"
                },
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 604800, // 7 days
                AllowOfflineAccess = true,
                RequireConsent = false
            },

            // Admin BFF - Backend for Frontend for Admin Portal
            new Client
            {
                ClientId = "admin-bff",
                ClientName = "Admin BFF",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false, // Public client

                // BFF redirect URIs
                RedirectUris = {
                    "https://localhost:5101/signin-oidc"
                },
                PostLogoutRedirectUris = {
                    "https://localhost:5101/signout-callback-oidc"
                },
                FrontChannelLogoutUri = "https://localhost:5101/signout-oidc",

                AllowedCorsOrigins = {
                    "https://localhost:5101"
                },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "vendo.api.full_access",
                    "roles",
                    "tenant"
                },

                // Admin portal has shorter token lifetime for security
                AccessTokenLifetime = 1800, // 30 minutes
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 43200, // 12 hours
                AllowOfflineAccess = true, // For refresh tokens
                RequireConsent = false,

                // BFF-specific settings
                AlwaysIncludeUserClaimsInIdToken = true,
                UpdateAccessTokenClaimsOnRefresh = true
            }
        };
}
