import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

/**
 * OAuth2/OIDC Configuration for IdentityServer
 *
 * This configuration uses Authorization Code Flow with PKCE (Proof Key for Code Exchange)
 * which is the recommended flow for Single Page Applications (SPAs).
 *
 * PKCE provides additional security by generating a code verifier and challenge
 * to prevent authorization code interception attacks.
 */
export const authConfig: AuthConfig = {
  // Identity Server URL (issuer)
  issuer: environment.identityUrl,

  // URL to redirect to after login
  redirectUri: window.location.origin + '/auth/callback',

  // URL to redirect to after logout
  postLogoutRedirectUri: window.location.origin + '/auth/signout-callback',

  // The SPA's client id (configured in IdentityServer)
  clientId: 'spa',

  // Set the response type to 'code' for Authorization Code Flow
  responseType: 'code',

  // Requested scopes
  // - openid: Required for OpenID Connect
  // - profile: User profile information
  // - email: User email address
  // - roles: User roles for authorization
  // - tenant: Tenant information for multi-tenancy
  // - vendo.api.full_access: Full access to Vendo API
  scope: 'openid profile email roles tenant vendo.api.full_access',

  // Show debug information in console (disable in production)
  showDebugInformation: !environment.production,

  // Disable HTTPS requirement for development (MUST be true in production)
  requireHttps: environment.production,

  // Use silent refresh via iframe to renew tokens before expiration
  useSilentRefresh: true,

  // Timeout for silent refresh in milliseconds
  silentRefreshTimeout: 5000,

  // Show iframe for silent refresh in DOM (for debugging only)
  silentRefreshShowIFrame: false,

  // URL for silent refresh
  silentRefreshRedirectUri: window.location.origin + '/assets/silent-refresh.html',

  // Time offset to start token refresh before expiration (in seconds)
  // This ensures tokens are refreshed before they expire
  timeoutFactor: 0.75,

  // Session checks interval (in milliseconds)
  sessionChecksEnabled: true,

  // Clear hash after login to avoid exposing tokens in URL
  clearHashAfterLogin: true,

  // Disable id_token nonce verification for development
  // (MUST be enabled in production for security)
  disableIdTokenNonceClaim: false,

  // Revoke refresh token on logout
  revokeTokenOnLogout: true,

  // Token endpoint for revoking tokens
  revocationEndpoint: environment.identityUrl + '/connect/revocation',

  // Custom query parameters to pass during authorization
  // These can be used to pass additional context like user type
  customQueryParams: {}
};

/**
 * Get OAuth configuration with role-specific customization
 * @param role Optional role to customize the login experience
 */
export function getAuthConfigForRole(role?: string): AuthConfig {
  const config = { ...authConfig };

  if (role) {
    // Add role as custom parameter to help IdentityServer customize login UI
    config.customQueryParams = {
      ...config.customQueryParams,
      user_type: role
    };
  }

  return config;
}
