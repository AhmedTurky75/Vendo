# OAuth2/OIDC Endpoints Reference

## Overview

This document provides a complete reference for all OAuth2/OIDC endpoints in the Vendo Identity Service. The service is built on Duende IdentityServer 7 and fully supports standard OAuth2 and OpenID Connect protocols.

---

## Discovery Endpoint

### GET /.well-known/openid-configuration

Returns the OpenID Connect discovery document containing all configuration information.

**Request:**
```bash
curl https://localhost:5001/.well-known/openid-configuration
```

**Response:**
```json
{
  "issuer": "https://localhost:5001",
  "authorization_endpoint": "https://localhost:5001/connect/authorize",
  "token_endpoint": "https://localhost:5001/connect/token",
  "userinfo_endpoint": "https://localhost:5001/connect/userinfo",
  "end_session_endpoint": "https://localhost:5001/connect/endsession",
  "jwks_uri": "https://localhost:5001/.well-known/openid-configuration/jwks",
  "grant_types_supported": [
    "authorization_code",
    "client_credentials",
    "password",
    "refresh_token"
  ],
  "response_types_supported": [
    "code",
    "token",
    "id_token"
  ],
  "scopes_supported": [
    "openid",
    "profile",
    "email",
    "roles",
    "tenant",
    "vendo.api.full_access",
    "vendo.api.read",
    "vendo.api.write"
  ],
  "token_endpoint_auth_methods_supported": [
    "client_secret_post",
    "client_secret_basic"
  ],
  "code_challenge_methods_supported": [
    "S256"
  ]
}
```

---

## Authorization Endpoint

### GET /connect/authorize

Initiates the authorization code flow. Used for interactive user authentication.

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `client_id` | Yes | Client identifier |
| `redirect_uri` | Yes | Where to redirect after authentication |
| `response_type` | Yes | Must be `code` |
| `scope` | Yes | Space-separated list of scopes |
| `state` | Recommended | Opaque value for CSRF protection |
| `code_challenge` | Required for PKCE | Base64-URL encoded SHA256 hash |
| `code_challenge_method` | Required for PKCE | Must be `S256` |
| `nonce` | Recommended | Random value for replay protection |
| `prompt` | Optional | `login`, `consent`, or `none` |

**Example (Authorization Code with PKCE):**
```
https://localhost:5001/connect/authorize?
  client_id=spa&
  redirect_uri=https://localhost:4200/auth/callback&
  response_type=code&
  scope=openid profile email vendo.api.full_access roles tenant&
  state=random-state-value&
  code_challenge=BASE64URL(SHA256(code_verifier))&
  code_challenge_method=S256&
  nonce=random-nonce-value
```

**Success Response:**

Redirects to `redirect_uri` with:
```
https://localhost:4200/auth/callback?
  code=AUTHORIZATION_CODE&
  state=random-state-value
```

**Error Response:**

Redirects to `redirect_uri` with:
```
https://localhost:4200/auth/callback?
  error=invalid_request&
  error_description=Missing+required+parameter
```

---

## Token Endpoint

### POST /connect/token

Exchanges authorization code or credentials for access tokens.

**Content-Type:** `application/x-www-form-urlencoded`

### Grant Type: Authorization Code

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | Must be `authorization_code` |
| `code` | Yes | Authorization code from authorize endpoint |
| `redirect_uri` | Yes | Must match the one used in authorize request |
| `client_id` | Yes | Client identifier |
| `client_secret` | Conditional | Required if client has a secret |
| `code_verifier` | Required for PKCE | Original random value |

**Example:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=authorization_code" \
  -d "code=AUTHORIZATION_CODE" \
  -d "redirect_uri=https://localhost:4200/auth/callback" \
  -d "client_id=spa" \
  -d "code_verifier=ORIGINAL_CODE_VERIFIER"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "refresh_token": "CfDJ8...",
  "id_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "scope": "openid profile email vendo.api.full_access roles tenant"
}
```

### Grant Type: Resource Owner Password (Legacy)

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | Must be `password` |
| `username` | Yes | User's username |
| `password` | Yes | User's password |
| `client_id` | Yes | Client identifier |
| `client_secret` | Yes | Client secret |
| `scope` | Yes | Space-separated list of scopes |

**Example:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "username=admin" \
  -d "password=Admin@123" \
  -d "client_id=client" \
  -d "client_secret=secret" \
  -d "scope=openid profile email vendo.api.full_access roles tenant"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "refresh_token": "CfDJ8...",
  "scope": "openid profile email vendo.api.full_access roles tenant"
}
```

### Grant Type: Client Credentials

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | Must be `client_credentials` |
| `client_id` | Yes | Client identifier |
| `client_secret` | Yes | Client secret |
| `scope` | Yes | Space-separated list of scopes |

**Example:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=service" \
  -d "client_secret=service-secret" \
  -d "scope=vendo.api.full_access"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "scope": "vendo.api.full_access"
}
```

### Grant Type: Refresh Token

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `grant_type` | Yes | Must be `refresh_token` |
| `refresh_token` | Yes | Refresh token from previous response |
| `client_id` | Yes | Client identifier |
| `client_secret` | Conditional | Required if client has a secret |

**Example:**
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token" \
  -d "refresh_token=CfDJ8..." \
  -d "client_id=spa"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "refresh_token": "CfDJ8...",
  "scope": "openid profile email vendo.api.full_access roles tenant"
}
```

### Error Responses

**Invalid Grant:**
```json
{
  "error": "invalid_grant",
  "error_description": "Invalid username or password"
}
```

**Invalid Client:**
```json
{
  "error": "invalid_client",
  "error_description": "Invalid client credentials"
}
```

**Invalid Scope:**
```json
{
  "error": "invalid_scope",
  "error_description": "Requested scope is invalid"
}
```

**Unauthorized Client:**
```json
{
  "error": "unauthorized_client",
  "error_description": "Client is not authorized to use this grant type"
}
```

---

## UserInfo Endpoint

### GET /connect/userinfo

Returns claims about the authenticated user.

**Authorization:** Bearer token required

**Example:**
```bash
curl -X GET https://localhost:5001/connect/userinfo \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6..."
```

**Response:**
```json
{
  "sub": "123e4567-e89b-12d3-a456-426614174000",
  "name": "John Doe",
  "given_name": "John",
  "family_name": "Doe",
  "email": "john.doe@vendo.com",
  "email_verified": true,
  "username": "johndoe",
  "role": ["User", "Admin"],
  "tenant_id": "default",
  "tenant_name": "Default Tenant",
  "updated_at": "2025-10-22T10:30:00Z"
}
```

---

## End Session Endpoint

### GET /connect/endsession

Logs out the user and optionally redirects back to the application.

**Parameters:**

| Parameter | Optional | Description |
|-----------|----------|-------------|
| `id_token_hint` | Yes | ID token received during login |
| `post_logout_redirect_uri` | Yes | Where to redirect after logout |
| `state` | Yes | Opaque value returned to redirect URI |

**Example:**
```
https://localhost:5001/connect/endsession?
  id_token_hint=eyJhbGciOiJSUzI1NiIsImtpZCI6...&
  post_logout_redirect_uri=https://localhost:4200/auth/signout-callback&
  state=random-state-value
```

**Response:**

Redirects to `post_logout_redirect_uri`:
```
https://localhost:4200/auth/signout-callback?state=random-state-value
```

---

## Token Revocation Endpoint

### POST /connect/revocation

Revokes an access or refresh token.

**Content-Type:** `application/x-www-form-urlencoded`

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `token` | Yes | Token to revoke |
| `token_type_hint` | No | `access_token` or `refresh_token` |
| `client_id` | Yes | Client identifier |
| `client_secret` | Conditional | Required if client has a secret |

**Example:**
```bash
curl -X POST https://localhost:5001/connect/revocation \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "token=CfDJ8..." \
  -d "token_type_hint=refresh_token" \
  -d "client_id=spa"
```

**Response:**

HTTP 200 OK (no body)

---

## Token Introspection Endpoint

### POST /connect/introspect

Validates a token and returns its metadata.

**Content-Type:** `application/x-www-form-urlencoded`

**Authorization:** Client credentials required

**Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `token` | Yes | Token to introspect |
| `token_type_hint` | No | `access_token` or `refresh_token` |

**Example:**
```bash
curl -X POST https://localhost:5001/connect/introspect \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -u "service:service-secret" \
  -d "token=eyJhbGciOiJSUzI1NiIsImtpZCI6..."
```

**Response (Active Token):**
```json
{
  "active": true,
  "sub": "123e4567-e89b-12d3-a456-426614174000",
  "client_id": "spa",
  "token_type": "access_token",
  "exp": 1234567890,
  "iat": 1234564290,
  "nbf": 1234564290,
  "aud": "vendo.api",
  "iss": "https://localhost:5001",
  "scope": "openid profile email vendo.api.full_access"
}
```

**Response (Inactive Token):**
```json
{
  "active": false
}
```

---

## JWKS Endpoint

### GET /.well-known/openid-configuration/jwks

Returns the JSON Web Key Set used to verify token signatures.

**Example:**
```bash
curl https://localhost:5001/.well-known/openid-configuration/jwks
```

**Response:**
```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "8E0F...",
      "e": "AQAB",
      "n": "xGOr...",
      "alg": "RS256"
    }
  ]
}
```

---

## Custom Account Endpoints

### POST /api/account/register

Registers a new user account.

**Authorization:** None (public endpoint)

**Request Body:**
```json
{
  "username": "newuser",
  "email": "newuser@vendo.com",
  "password": "NewUser@123",
  "firstName": "New",
  "lastName": "User"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "username": "newuser",
    "email": "newuser@vendo.com",
    "firstName": "New",
    "lastName": "User",
    "isActive": true,
    "roles": ["User"],
    "createdAt": "2025-10-22T10:30:00Z",
    "updatedAt": "2025-10-22T10:30:00Z"
  }
}
```

### GET /api/account/profile

Gets the current user's profile.

**Authorization:** Bearer token required

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "username": "johndoe",
    "email": "john.doe@vendo.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "roles": ["User", "Admin"],
    "createdAt": "2025-10-22T10:30:00Z",
    "updatedAt": "2025-10-22T10:30:00Z"
  }
}
```

### PUT /api/account/profile

Updates the current user's profile.

**Authorization:** Bearer token required

**Request Body:**
```json
{
  "email": "newemail@vendo.com",
  "firstName": "John",
  "lastName": "Smith"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "username": "johndoe",
    "email": "newemail@vendo.com",
    "firstName": "John",
    "lastName": "Smith",
    "isActive": true,
    "roles": ["User"],
    "createdAt": "2025-10-22T10:30:00Z",
    "updatedAt": "2025-10-22T10:40:00Z"
  }
}
```

### POST /api/account/change-password

Changes the current user's password.

**Authorization:** Bearer token required

**Request Body:**
```json
{
  "currentPassword": "OldPassword@123",
  "newPassword": "NewPassword@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Password changed successfully"
  }
}
```

### POST /api/account/forgot-password

Initiates password reset process.

**Authorization:** None (public endpoint)

**Request Body:**
```json
{
  "email": "john.doe@vendo.com"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "If the email exists, a password reset link has been sent. Please check your email."
  }
}
```

### POST /api/account/reset-password

Resets password using a reset token.

**Authorization:** None (public endpoint)

**Request Body:**
```json
{
  "email": "john.doe@vendo.com",
  "token": "reset-token-from-email",
  "newPassword": "NewPassword@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Password has been reset successfully"
  }
}
```

---

## HTTP Status Codes

| Status Code | Meaning |
|-------------|---------|
| 200 | Success |
| 201 | Created (registration successful) |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (invalid or missing token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Rate Limiting

To prevent abuse, consider implementing rate limiting on these endpoints:

- `/connect/token`: 10 requests per minute per IP
- `/api/account/register`: 5 requests per hour per IP
- `/api/account/forgot-password`: 3 requests per hour per IP
- `/connect/authorize`: 20 requests per minute per client

---

## Security Headers

All responses include security headers:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

---

## CORS Configuration

CORS is configured for the following origins (development):

- `https://localhost:4200` (SPA)
- `https://localhost:4300` (Admin Portal)
- `https://localhost:4400` (Merchant Portal)
- `https://localhost:5001` (Identity Server)
- `https://localhost:5002` (Interactive App)

In production, update `appsettings.Production.json` with actual origins.

---

## Testing

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Click **Authorize**
3. Select scopes and click **Authorize**
4. Login with test credentials
5. Test endpoints

### Using cURL

See examples above for each endpoint.

### Using Postman

Import the IdentityServer discovery document to automatically create a collection:

1. Create new Request
2. Under Authorization, select "OAuth 2.0"
3. Configure Token URL: `https://localhost:5001/connect/token`
4. Configure Auth URL: `https://localhost:5001/connect/authorize`
5. Set Client ID, Scopes, etc.

---

## Additional Resources

- [OAuth2 Migration Guide](./OAUTH2-MIGRATION-GUIDE.md)
- [API Testing Guide](./API-TESTING-GUIDE.md)
- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/identityserver/v7)
- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
