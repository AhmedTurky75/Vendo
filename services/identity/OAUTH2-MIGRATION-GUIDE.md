# OAuth2/OIDC Migration Guide

## Overview

This guide helps you migrate from the deprecated custom `/api/account/login` endpoint to the standard IdentityServer OAuth2/OIDC flows. The custom endpoint will be removed in a future version, so please plan your migration accordingly.

## Why Migrate?

1. **Industry Standard**: OAuth2/OIDC are industry-standard protocols supported by all major platforms
2. **Better Security**: Proper token flows with PKCE, refresh tokens, and standard security practices
3. **Interoperability**: Works with standard OAuth2 libraries and tools
4. **Feature Rich**: Support for single sign-on, token refresh, logout, and more
5. **Compliance**: Meets security requirements outlined in `07_SECURITY_AND_COMPLIANCE.md`

## Migration Options

### Option 1: Authorization Code with PKCE (Recommended for Web/Mobile)

**Best for**: Single Page Applications (SPAs), Mobile Apps, Server-side Web Apps

**Security**: Highest - No client secret in frontend, PKCE prevents authorization code interception

**Implementation Complexity**: Medium - Requires OAuth2 library

#### Flow Diagram
```
1. User clicks "Login"
2. App redirects to /connect/authorize with PKCE challenge
3. User authenticates on IdentityServer
4. IdentityServer redirects back with authorization code
5. App exchanges code for tokens at /connect/token
6. App receives access_token, id_token, and refresh_token
```

#### Example: Angular/React SPA

**Install Library:**
```bash
# For Angular
npm install angular-oauth2-oidc

# For React
npm install oidc-client-ts
```

**Configure (Angular):**
```typescript
import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {
  issuer: 'https://localhost:5001',
  redirectUri: window.location.origin + '/auth/callback',
  clientId: 'spa',
  responseType: 'code',
  scope: 'openid profile email vendo.api.full_access roles tenant',
  showDebugInformation: true,
  requireHttps: false // Only for development
};
```

**Configure (React with oidc-client-ts):**
```typescript
import { UserManager } from 'oidc-client-ts';

const userManager = new UserManager({
  authority: 'https://localhost:5001',
  client_id: 'spa',
  redirect_uri: 'https://localhost:4200/auth/callback',
  response_type: 'code',
  scope: 'openid profile email vendo.api.full_access roles tenant',
  post_logout_redirect_uri: 'https://localhost:4200/auth/signout-callback',
});
```

**Login (Angular):**
```typescript
import { OAuthService } from 'angular-oauth2-oidc';

constructor(private oauthService: OAuthService) {}

login() {
  this.oauthService.initCodeFlow();
}

async getProfile() {
  const claims = this.oauthService.getIdentityClaims();
  console.log('User:', claims);

  // Make API calls with token
  const token = this.oauthService.getAccessToken();
  // Use token in HTTP headers
}
```

**Login (React):**
```typescript
// Login
const login = async () => {
  await userManager.signinRedirect();
};

// Handle callback
const handleCallback = async () => {
  const user = await userManager.signinRedirectCallback();
  console.log('User:', user);
};

// Make API calls
const callApi = async () => {
  const user = await userManager.getUser();
  const response = await fetch('https://localhost:5001/api/account/profile', {
    headers: {
      'Authorization': `Bearer ${user.access_token}`
    }
  });
};
```

#### Example: Mobile App (React Native)

**Install Library:**
```bash
npm install react-native-app-auth
```

**Configure:**
```typescript
import { authorize } from 'react-native-app-auth';

const config = {
  issuer: 'https://localhost:5001',
  clientId: 'mobile',
  redirectUrl: 'com.vendo.app://callback',
  scopes: ['openid', 'profile', 'email', 'vendo.api.full_access', 'roles', 'tenant'],
  usePKCE: true
};

const authState = await authorize(config);
// authState contains: accessToken, refreshToken, idToken
```

---

### Option 2: Resource Owner Password Credentials (Legacy/Backward Compatible)

**Best for**: Legacy applications during migration, testing, internal tools

**Security**: Lower - Requires sending username/password to backend

**Implementation Complexity**: Low - Simple HTTP POST

**Note**: This flow is maintained for backward compatibility but should be migrated to Authorization Code + PKCE when possible.

#### Before (Deprecated Custom Endpoint):
```bash
curl -X POST "https://localhost:5001/api/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

#### After (IdentityServer Resource Owner Password Flow):
```bash
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin@123&client_id=client&client_secret=secret&scope=openid profile email vendo.api.full_access roles tenant"
```

#### JavaScript/TypeScript Example:
```typescript
async function login(username: string, password: string) {
  const params = new URLSearchParams({
    grant_type: 'password',
    username: username,
    password: password,
    client_id: 'client',
    client_secret: 'secret',
    scope: 'openid profile email vendo.api.full_access roles tenant'
  });

  const response = await fetch('https://localhost:5001/connect/token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: params
  });

  const data = await response.json();

  if (response.ok) {
    // Store tokens
    localStorage.setItem('access_token', data.access_token);
    localStorage.setItem('refresh_token', data.refresh_token);

    return {
      success: true,
      accessToken: data.access_token,
      refreshToken: data.refresh_token,
      expiresIn: data.expires_in
    };
  } else {
    return {
      success: false,
      error: data.error_description || 'Authentication failed'
    };
  }
}
```

---

### Option 3: Client Credentials (Service-to-Service)

**Best for**: Backend services, microservices, API-to-API communication

**Security**: High - No user context, machine-to-machine authentication

**Implementation Complexity**: Low - Simple HTTP POST

#### Example:
```bash
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=service&client_secret=service-secret&scope=vendo.api.full_access"
```

#### C# Example:
```csharp
using IdentityModel.Client;

var client = new HttpClient();
var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");

var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,
    ClientId = "service",
    ClientSecret = "service-secret",
    Scope = "vendo.api.full_access"
});

if (!tokenResponse.IsError)
{
    var accessToken = tokenResponse.AccessToken;
    // Use token for API calls
}
```

---

## Token Usage

### Making API Calls

After obtaining an access token, include it in the Authorization header:

```typescript
const response = await fetch('https://localhost:5001/api/account/profile', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
```

### Refreshing Tokens

When the access token expires, use the refresh token to get a new one:

```bash
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token&refresh_token=YOUR_REFRESH_TOKEN&client_id=spa"
```

```typescript
async function refreshToken(refreshToken: string) {
  const params = new URLSearchParams({
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
    client_id: 'spa'
  });

  const response = await fetch('https://localhost:5001/connect/token', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: params
  });

  const data = await response.json();

  if (response.ok) {
    localStorage.setItem('access_token', data.access_token);
    localStorage.setItem('refresh_token', data.refresh_token);
    return data.access_token;
  }
}
```

---

## Available Clients

| Client ID | Name | Grant Type | Use Case | Secret Required |
|-----------|------|------------|----------|----------------|
| `swagger` | Swagger UI | Authorization Code + PKCE | API testing | No |
| `spa` | Single Page App | Authorization Code + PKCE | Web applications | No |
| `mobile` | Mobile App | Authorization Code + PKCE | Mobile applications | No |
| `interactive` | Web Application | Authorization Code + PKCE | Server-side web apps | Yes |
| `admin-portal` | Admin Portal | Authorization Code + PKCE | Admin interface | No |
| `merchant-portal` | Merchant Portal | Authorization Code + PKCE | Merchant interface | No |
| `client` | Legacy Client | Resource Owner Password | Legacy apps (deprecated) | Yes |
| `service` | Backend Service | Client Credentials | Service-to-service | Yes |

---

## Available Scopes

| Scope | Description | Required For |
|-------|-------------|--------------|
| `openid` | OpenID Connect | User authentication |
| `profile` | User profile information | User details |
| `email` | User email address | Email access |
| `roles` | User roles | Authorization |
| `tenant` | Tenant information | Multi-tenancy |
| `vendo.api.full_access` | Full API access | All operations |
| `vendo.api.read` | Read-only API access | Read operations |
| `vendo.api.write` | Write API access | Write operations |

---

## Token Claims

### Access Token Claims

After successful authentication, the access token contains:

```json
{
  "sub": "user-id-guid",
  "name": "John Doe",
  "email": "john@example.com",
  "username": "johndoe",
  "role": ["User", "Admin"],
  "tenant_id": "default",
  "tenant_name": "Default Tenant",
  "client_id": "spa",
  "scope": ["openid", "profile", "email", "vendo.api.full_access"],
  "exp": 1234567890,
  "iss": "https://localhost:5001",
  "aud": "vendo.api"
}
```

### Accessing Claims in Code

**JavaScript:**
```typescript
import jwtDecode from 'jwt-decode';

const token = localStorage.getItem('access_token');
const claims = jwtDecode(token);
console.log('User ID:', claims.sub);
console.log('Email:', claims.email);
console.log('Roles:', claims.role);
console.log('Tenant:', claims.tenant_id);
```

**C#:**
```csharp
// In a controller with [Authorize] attribute
var userId = User.FindFirst("sub")?.Value;
var email = User.FindFirst("email")?.Value;
var roles = User.FindAll("role").Select(c => c.Value);
var tenantId = User.FindFirst("tenant_id")?.Value;
```

---

## IdentityServer Discovery

IdentityServer provides a discovery endpoint that lists all available endpoints and configuration:

```bash
curl https://localhost:5001/.well-known/openid-configuration
```

Response includes:
- `authorization_endpoint`: `/connect/authorize`
- `token_endpoint`: `/connect/token`
- `userinfo_endpoint`: `/connect/userinfo`
- `end_session_endpoint`: `/connect/endsession`
- `jwks_uri`: `/connect/jwks`
- Supported scopes, grant types, and response types

---

## Migration Checklist

### Phase 1: Preparation
- [ ] Review this migration guide
- [ ] Choose appropriate OAuth2 flow for your application
- [ ] Install required OAuth2/OIDC libraries
- [ ] Update redirect URIs in IdentityServerConfig if needed
- [ ] Test authentication in development environment

### Phase 2: Implementation
- [ ] Implement OAuth2/OIDC authentication in your application
- [ ] Update token storage mechanism (secure storage)
- [ ] Implement token refresh logic
- [ ] Update API calls to use new tokens
- [ ] Implement logout functionality
- [ ] Test all authentication scenarios

### Phase 3: Testing
- [ ] Test login flow
- [ ] Test token refresh
- [ ] Test logout
- [ ] Test API calls with new tokens
- [ ] Test error scenarios (invalid credentials, expired tokens, etc.)
- [ ] Verify claims are correct (roles, email, tenant, etc.)

### Phase 4: Deployment
- [ ] Deploy updated application to staging
- [ ] Perform integration testing
- [ ] Monitor logs for any issues
- [ ] Deploy to production
- [ ] Remove deprecated endpoint calls

### Phase 5: Cleanup (Future)
- [ ] Remove custom login endpoint code (after deprecation period)
- [ ] Update documentation
- [ ] Remove legacy client configurations

---

## Testing with Swagger

Swagger UI is already configured with OAuth2:

1. Navigate to `https://localhost:5001/swagger`
2. Click the **Authorize** button
3. Select all scopes you need
4. Click **Authorize** (you'll be redirected to login page)
5. Login with test credentials (e.g., `admin` / `Admin@123`)
6. After successful login, you can test all protected endpoints

---

## Common Issues and Solutions

### Issue: CORS Error

**Symptom**: `Access to fetch at 'https://localhost:5001/connect/token' has been blocked by CORS policy`

**Solution**: Ensure your origin is in the `Cors:AllowedOrigins` configuration in `appsettings.json`

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:4200"
    ]
  }
}
```

### Issue: Redirect URI Mismatch

**Symptom**: `invalid_redirect_uri`

**Solution**: Ensure the redirect URI in your client configuration matches exactly what you're using in your application. Check `IdentityServerConfig.cs` and update the client's `RedirectUris` array.

### Issue: Invalid Scope

**Symptom**: `invalid_scope`

**Solution**: Ensure the scopes you're requesting are configured in `IdentityServerConfig.cs` and are allowed for your client.

### Issue: Token Expired

**Symptom**: `401 Unauthorized` when calling API

**Solution**: Implement token refresh logic. If the access token is expired, use the refresh token to get a new one.

### Issue: Invalid Client

**Symptom**: `invalid_client`

**Solution**: Ensure the client ID and secret (if required) are correct. Check `IdentityServerConfig.cs` for available clients.

---

## Security Best Practices

1. **Use HTTPS**: Always use HTTPS in production
2. **Secure Token Storage**:
   - Web: Use `httpOnly` cookies or secure storage
   - Mobile: Use secure keychain/keystore
   - Never store tokens in localStorage for sensitive applications
3. **Short Token Lifetime**: Use short-lived access tokens (1 hour or less)
4. **Token Refresh**: Implement automatic token refresh
5. **PKCE**: Always use PKCE for Authorization Code flow
6. **Client Secret**: Never expose client secrets in frontend code
7. **Validate Tokens**: Always validate tokens on the backend
8. **Logout**: Implement proper logout that revokes tokens
9. **Rate Limiting**: Implement rate limiting on token endpoints
10. **Monitor**: Monitor authentication failures and suspicious activity

---

## Production Considerations

Before going to production:

1. **Update Client Secrets**: Replace all default secrets with strong, randomly generated secrets
2. **Configure HTTPS**: Set `RequireHttpsMetadata = true`
3. **Use Production Signing Certificate**: Replace developer signing credential
4. **Update CORS**: Configure production origins in `appsettings.Production.json`
5. **Enable Logging**: Configure structured logging (Serilog + Application Insights)
6. **Rate Limiting**: Implement rate limiting for token endpoint
7. **MFA**: Consider implementing Multi-Factor Authentication
8. **Token Revocation**: Implement token revocation for logout
9. **Refresh Token Rotation**: Use one-time refresh tokens
10. **Monitoring**: Set up monitoring and alerting for authentication failures

---

## Additional Resources

- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/identityserver/v7)
- [OAuth 2.0 Specification](https://oauth.net/2/)
- [OpenID Connect Specification](https://openid.net/connect/)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [angular-oauth2-oidc Documentation](https://github.com/manfredsteyer/angular-oauth2-oidc)
- [oidc-client-ts Documentation](https://github.com/authts/oidc-client-ts)

---

## Support

If you encounter issues during migration:

1. Check the logs: `Duende.IdentityServer` logs provide detailed information
2. Review the [API Testing Guide](./API-TESTING-GUIDE.md)
3. Test with Swagger UI first to verify server configuration
4. Check the [Security and Compliance Guide](../../07_SECURITY_AND_COMPLIANCE.md)
5. Contact the Vendo development team for assistance

---

## Timeline

- **Now**: Custom `/api/account/login` endpoint is deprecated but still functional
- **Q1 2026**: Custom endpoint will log warnings for all usage
- **Q2 2026**: Custom endpoint will be removed (breaking change)

Please plan your migration accordingly to avoid service disruption.
