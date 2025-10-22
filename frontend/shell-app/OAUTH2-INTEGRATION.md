# OAuth2/OIDC Integration - Frontend Implementation

## Overview

This document describes the OAuth2/OIDC integration implemented in the Angular frontend application. The integration uses **Authorization Code Flow with PKCE (Proof Key for Code Exchange)**, which is the recommended and most secure flow for Single Page Applications (SPAs).

## Table of Contents

1. [What Was Implemented](#what-was-implemented)
2. [Library and Approach](#library-and-approach)
3. [Token Storage and Security](#token-storage-and-security)
4. [Login Flow](#login-flow)
5. [Changes to Existing Components](#changes-to-existing-components)
6. [Configuration](#configuration)
7. [Testing the Integration](#testing-the-integration)
8. [Troubleshooting](#troubleshooting)
9. [Future Enhancements](#future-enhancements)

---

## What Was Implemented

### 1. OAuth2/OIDC Library Integration
- Installed and configured `angular-oauth2-oidc` library (v18.0.0+)
- Integrated with Duende IdentityServer 7
- Implemented Authorization Code + PKCE flow

### 2. Core Authentication Service
- **File**: `/src/app/core/services/auth.service.ts`
- Completely rewritten to use OAuth2/OIDC
- Features:
  - Automatic discovery document loading
  - PKCE code generation and verification
  - Automatic token refresh
  - Silent token refresh via hidden iframe
  - Token validation and expiration handling
  - User profile extraction from ID token claims
  - Role-based access control

### 3. OAuth Configuration
- **File**: `/src/app/core/config/oauth.config.ts`
- Centralized OIDC configuration
- Environment-specific settings
- Role-specific customization support

### 4. Callback Components
- **AuthCallbackComponent**: Handles OAuth redirect after successful login
- **SignoutCallbackComponent**: Handles redirect after logout
- Both components provide user feedback during the process

### 5. Updated Guards and Interceptors
- **AuthGuard**: Enhanced to check OIDC token validity and user roles
- **AuthInterceptor**:
  - Adds access token to API requests
  - Handles token refresh on 401 responses
  - Queues requests during token refresh

### 6. Login Components
- Updated all three login components:
  - CustomerLoginComponent
  - AdminLoginComponent
  - MerchantLoginComponent
- Now redirect to IdentityServer instead of using deprecated custom endpoint

### 7. Environment Configuration
- Added OIDC settings to `environment.ts` and `environment.prod.ts`
- Configured issuer, client ID, scopes, and other OAuth parameters

---

## Library and Approach

### Library: angular-oauth2-oidc

**Why this library?**
- Official Angular OAuth2/OIDC library
- Actively maintained with 4.6K+ stars on GitHub
- Fully compliant with OAuth2 and OpenID Connect specifications
- Built-in support for PKCE
- Automatic token refresh capabilities
- Well-documented and widely adopted

**Installation:**
```bash
npm install angular-oauth2-oidc --save
```

### Approach: Authorization Code + PKCE

**Why PKCE?**
1. **Enhanced Security**: Prevents authorization code interception attacks
2. **No Client Secret**: Safe for public clients (SPAs) where secrets cannot be kept secure
3. **Industry Standard**: Recommended by OAuth 2.0 Security Best Current Practice
4. **Modern**: Required for OAuth 2.1

**How PKCE Works:**
1. App generates a random `code_verifier` (43-128 chars)
2. App creates `code_challenge` = BASE64URL(SHA256(code_verifier))
3. App sends `code_challenge` to authorization endpoint
4. After user authenticates, app receives authorization code
5. App exchanges code + original `code_verifier` for tokens
6. Server verifies that SHA256(code_verifier) matches the original challenge

---

## Token Storage and Security

### Storage Strategy: SessionStorage

**Why sessionStorage instead of localStorage?**
1. **Automatic Cleanup**: Cleared when browser tab/window is closed
2. **Tab Isolation**: Tokens are not shared between tabs
3. **Reduced Attack Surface**: More secure than localStorage for sensitive data
4. **Compliance**: Better aligns with security best practices

**Alternative Considered: HttpOnly Cookies via BFF**

A Backend for Frontend (BFF) pattern with HttpOnly cookies provides even better security:
- **Pros**:
  - Tokens completely inaccessible to JavaScript
  - Protection against XSS attacks
  - Best security for sensitive applications
- **Cons**:
  - Requires additional backend infrastructure
  - More complex architecture
  - Overhead for simple applications

**Current Implementation**: We use sessionStorage as it provides a good balance of security and simplicity for SPAs. For production applications with high security requirements, consider implementing a BFF.

### Token Types Stored

1. **Access Token**: JWT used for API authorization (1 hour lifetime)
2. **Refresh Token**: Used to obtain new access tokens (longer lifetime)
3. **ID Token**: Contains user identity claims

### Security Features

1. **PKCE**: Prevents authorization code interception
2. **State Parameter**: CSRF protection
3. **Nonce**: Replay attack prevention
4. **Token Validation**: Signature and expiration verification
5. **Automatic Refresh**: Tokens refreshed before expiration
6. **Silent Refresh**: Uses hidden iframe to refresh without user interaction

---

## Login Flow

### Step-by-Step Flow

#### 1. User Clicks Login
```typescript
// In CustomerLoginComponent
onSubmit(): void {
  this.authService.login(UserRole.Customer);
}
```

#### 2. App Initiates Authorization Code Flow
```typescript
// In AuthService
login(role?: UserRole): void {
  // Configure with role-specific settings
  const config = getAuthConfigForRole(role);
  this.oauthService.configure(config);

  // Generate PKCE challenge and redirect
  this.oauthService.initCodeFlow();
}
```

**What happens:**
- Library generates random `code_verifier`
- Creates `code_challenge` = SHA256(code_verifier)
- Stores `code_verifier` in sessionStorage
- Redirects browser to IdentityServer

**Redirect URL Example:**
```
http://localhost:5001/connect/authorize?
  client_id=spa&
  redirect_uri=http://localhost:4200/auth/callback&
  response_type=code&
  scope=openid profile email vendo.api.full_access roles tenant&
  state=abc123&
  code_challenge=xyz789&
  code_challenge_method=S256&
  nonce=nonce123
```

#### 3. User Authenticates on IdentityServer
- User enters credentials on IdentityServer login page
- IdentityServer validates credentials
- User grants consent (if required)

#### 4. IdentityServer Redirects Back with Code
**Redirect URL:**
```
http://localhost:4200/auth/callback?
  code=AUTHORIZATION_CODE&
  state=abc123
```

#### 5. AuthCallbackComponent Handles Callback
```typescript
// In AuthCallbackComponent
ngOnInit(): void {
  this.authService.handleCallback().subscribe({
    next: (success) => {
      if (success) {
        this.redirectToRoleDashboard();
      }
    }
  });
}
```

#### 6. App Exchanges Code for Tokens
```typescript
// Automatically done by angular-oauth2-oidc
// POST http://localhost:5001/connect/token
{
  grant_type: 'authorization_code',
  code: 'AUTHORIZATION_CODE',
  redirect_uri: 'http://localhost:4200/auth/callback',
  client_id: 'spa',
  code_verifier: 'ORIGINAL_CODE_VERIFIER'
}
```

**IdentityServer verifies:**
- Authorization code is valid
- SHA256(code_verifier) matches stored code_challenge
- redirect_uri matches original request

**Response:**
```json
{
  "access_token": "eyJhbGc...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "refresh_token": "CfDJ8...",
  "id_token": "eyJhbGc...",
  "scope": "openid profile email vendo.api.full_access roles tenant"
}
```

#### 7. App Extracts User Profile
```typescript
// In AuthService
private loadUserProfile(): void {
  const claims = this.oauthService.getIdentityClaims();
  const user: User = {
    id: claims['sub'],
    email: claims['email'],
    role: this.mapClaimToRole(claims['role']),
    firstName: claims['given_name'],
    lastName: claims['family_name'],
    tenantId: claims['tenant_id']
  };
  this.currentUserSignal.set(user);
  this.isAuthenticatedSignal.set(true);
}
```

#### 8. User Redirected to Dashboard
Based on user role:
- **Admin** → `/admin/dashboard`
- **Merchant** → `/merchant/dashboard`
- **Customer** → `/shop`

---

## Changes to Existing Components

### 1. AuthService (`/core/services/auth.service.ts`)

**Before:**
- Used deprecated custom `/api/account/login` endpoint
- Stored tokens in localStorage
- Manual token management
- No automatic refresh

**After:**
- Uses OAuth2/OIDC Authorization Code + PKCE flow
- Integrated with angular-oauth2-oidc library
- Automatic token refresh via OAuthService
- Silent refresh via hidden iframe
- Secure sessionStorage for tokens
- Comprehensive token validation

**Key Methods:**
```typescript
// New OAuth-based methods
login(role?: UserRole): void
handleCallback(): Observable<boolean>
logout(): void
hasValidAccessToken(): boolean
refreshToken(): Observable<boolean>
getIdentityClaims(): Record<string, any>
```

### 2. Login Components

**Files Changed:**
- `/features/auth/customer-login/customer-login.component.ts`
- `/features/auth/admin-login/admin-login.component.ts`
- `/features/auth/merchant-login/merchant-login.component.ts`

**Changes:**
- `onSubmit()` now calls `authService.login(role)` which redirects to IdentityServer
- Form fields are kept for UI consistency but actual authentication happens on IdentityServer
- No direct password handling in frontend

**Note:** In future iterations, these components could be simplified to just a "Login with Vendo" button since credentials are entered on IdentityServer.

### 3. AuthGuard (`/core/guards/auth.guard.ts`)

**Before:**
```typescript
if (!this.authService.isAuthenticated()) {
  this.router.navigate(['/login/customer']);
  return false;
}
```

**After:**
```typescript
if (!this.authService.hasValidAccessToken()) {
  sessionStorage.setItem('redirect_url', state.url);
  return this.router.createUrlTree(['/login/customer']);
}
```

**Enhancements:**
- Checks OAuth token validity
- Stores attempted URL for post-login redirect
- Returns UrlTree for better Angular routing
- Waits for authentication initialization
- Supports checking multiple roles

### 4. AuthInterceptor (`/core/interceptors/auth.interceptor.ts`)

**Enhancements:**
- Gets token from OAuthService instead of localStorage
- Implements request queuing during token refresh
- Automatically refreshes expired tokens
- Filters out IdentityServer endpoints from token injection

**New Features:**
```typescript
// Request queuing during refresh
private isRefreshing = false;
private refreshTokenSubject: BehaviorSubject<any>;

// Handle 401 with automatic refresh
private handle401Error(request, next): Observable<HttpEvent<any>>
```

### 5. CoreModule (`/core/core.module.ts`)

**Addition:**
```typescript
imports: [
  OAuthModule.forRoot({
    resourceServer: {
      allowedUrls: ['http://localhost:5001/api'],
      sendAccessToken: true
    }
  })
]
```

### 6. AuthModule (`/features/auth/auth.module.ts`)

**New Components Added:**
- `AuthCallbackComponent`
- `SignoutCallbackComponent`

**New Routes:**
```typescript
{
  path: 'auth',
  children: [
    { path: 'callback', component: AuthCallbackComponent },
    { path: 'signout-callback', component: SignoutCallbackComponent }
  ]
}
```

### 7. Environment Files

**`environment.ts` and `environment.prod.ts`:**

Added OIDC configuration:
```typescript
oidc: {
  issuer: 'http://localhost:5001',
  clientId: 'spa',
  scope: 'openid profile email roles tenant vendo.api.full_access',
  responseType: 'code',
  requireHttps: false, // true in production
  showDebugInformation: true // false in production
}
```

---

## Configuration

### 1. IdentityServer Configuration

Ensure the `spa` client is configured in IdentityServer:

```csharp
new Client
{
    ClientId = "spa",
    ClientName = "Vendo SPA",
    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,
    RequireClientSecret = false, // Public client

    RedirectUris = {
        "http://localhost:4200/auth/callback"
    },
    PostLogoutRedirectUris = {
        "http://localhost:4200/auth/signout-callback"
    },
    AllowedCorsOrigins = {
        "http://localhost:4200"
    },

    AllowedScopes = {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        IdentityServerConstants.StandardScopes.Email,
        "roles",
        "tenant",
        "vendo.api.full_access"
    },

    AllowOfflineAccess = true, // Enable refresh tokens
    RefreshTokenUsage = TokenUsage.ReUse,
    RefreshTokenExpiration = TokenExpiration.Absolute,
    AbsoluteRefreshTokenLifetime = 2592000, // 30 days
    AccessTokenLifetime = 3600 // 1 hour
}
```

### 2. CORS Configuration

In IdentityServer `appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "http://localhost:4300",
      "http://localhost:4400"
    ]
  }
}
```

### 3. Frontend Configuration

**OAuth Config** (`/core/config/oauth.config.ts`):
```typescript
export const authConfig: AuthConfig = {
  issuer: 'http://localhost:5001',
  redirectUri: window.location.origin + '/auth/callback',
  postLogoutRedirectUri: window.location.origin + '/auth/signout-callback',
  clientId: 'spa',
  responseType: 'code',
  scope: 'openid profile email roles tenant vendo.api.full_access',
  showDebugInformation: true,
  requireHttps: false, // Only for dev
  useSilentRefresh: true
};
```

---

## Testing the Integration

### Prerequisites

1. **IdentityServer is running** at `http://localhost:5001`
2. **Test users exist** in IdentityServer database
3. **Angular app can reach** IdentityServer (CORS configured)

### Test User Credentials

Based on IdentityServer seed data:

| Username | Password | Role | Tenant |
|----------|----------|------|--------|
| admin | Admin@123 | Admin | Default |
| merchant | Merchant@123 | Merchant | Default |
| customer | Customer@123 | Customer | Default |

### Step-by-Step Testing

#### Test 1: Customer Login Flow

1. **Start the application:**
   ```bash
   cd /home/user/Vendo/frontend/shell-app
   npm start
   ```

2. **Navigate to customer login:**
   ```
   http://localhost:4200/login/customer
   ```

3. **Click "Sign In" button**
   - You should be redirected to IdentityServer login page
   - URL should be: `http://localhost:5001/connect/authorize?...`

4. **Enter credentials:**
   - Username: `customer`
   - Password: `Customer@123`

5. **Submit login form on IdentityServer**
   - You should be redirected back to: `http://localhost:4200/auth/callback?code=...`

6. **Wait for token exchange**
   - AuthCallbackComponent should show "Completing sign in..."
   - Tokens are exchanged automatically

7. **Verify redirect**
   - Should redirect to: `http://localhost:4200/shop`
   - User should be authenticated

8. **Check browser console**
   ```
   // Should see:
   Access token received
   User profile loaded
   ```

9. **Inspect sessionStorage**
   ```javascript
   // Open DevTools Console
   sessionStorage
   // Should see keys like:
   // - access_token_stored_at
   // - access_token
   // - id_token
   // - refresh_token
   ```

#### Test 2: Token Refresh

1. **Wait for token to near expiration** (or set shorter lifetime in IdentityServer)

2. **Watch browser console**
   - Should see: "Token is about to expire"
   - Silent refresh should occur automatically

3. **Verify token refreshed**
   ```javascript
   // In console
   sessionStorage.getItem('access_token')
   // Token should be different from original
   ```

#### Test 3: Protected Route Access

1. **Without authentication, try to access:**
   ```
   http://localhost:4200/admin/dashboard
   ```

2. **Verify redirect**
   - Should redirect to `/login/customer`
   - Attempted URL should be stored in sessionStorage

3. **Login as admin:**
   - Username: `admin`
   - Password: `Admin@123`

4. **Verify redirect after login**
   - Should redirect to `/admin/dashboard`

#### Test 4: Role-Based Access

1. **Login as customer**

2. **Try to access admin route:**
   ```
   http://localhost:4200/admin/dashboard
   ```

3. **Verify access denied**
   - Should redirect to `/unauthorized`
   - Console should log: "User does not have required role: admin"

#### Test 5: Logout Flow

1. **Login as any user**

2. **Click logout button** (if available in UI)
   - Or call: `authService.logout()` in console

3. **Verify IdentityServer logout**
   - Should redirect to IdentityServer end session endpoint
   - Then redirect to `/auth/signout-callback`

4. **Verify cleanup**
   - sessionStorage should be cleared
   - User should be logged out

5. **Verify redirect to login**
   - After 2 seconds, should redirect to `/login/customer`

#### Test 6: API Calls with Token

1. **Login as any user**

2. **Make API call** (in browser console):
   ```javascript
   fetch('http://localhost:5001/api/account/profile', {
     headers: {
       'Authorization': 'Bearer ' + sessionStorage.getItem('access_token')
     }
   })
   .then(res => res.json())
   .then(data => console.log(data));
   ```

3. **Verify response**
   - Should return user profile data
   - No 401 Unauthorized error

### Browser DevTools Debugging

**Network Tab:**
- Check redirect to `/connect/authorize`
- Check POST to `/connect/token`
- Check Authorization header on API calls

**Console:**
- Enable `showDebugInformation: true` in oauth.config.ts
- Watch for OAuth events
- Check for errors

**Application Tab (Storage):**
- Inspect sessionStorage keys
- View token contents (decode at jwt.io)
- Check expiration times

### Common Test Scenarios

#### Scenario: User refreshes page after login
**Expected:** User remains logged in, tokens are loaded from sessionStorage

#### Scenario: User opens app in new tab
**Expected:** User must login again (sessionStorage is tab-specific)

#### Scenario: User closes and reopens browser
**Expected:** User must login again (sessionStorage is cleared)

#### Scenario: Access token expires during API call
**Expected:** Interceptor automatically refreshes token and retries request

#### Scenario: Refresh token expires
**Expected:** User is redirected to login page

---

## Troubleshooting

### Issue 1: CORS Error

**Symptom:**
```
Access to fetch at 'http://localhost:5001/connect/token' has been blocked by CORS policy
```

**Solution:**
1. Check IdentityServer CORS configuration in `appsettings.json`
2. Ensure frontend origin is in `AllowedOrigins`
3. Restart IdentityServer after config changes

### Issue 2: Redirect URI Mismatch

**Symptom:**
```
invalid_redirect_uri
```

**Solution:**
1. Check client configuration in IdentityServer
2. Verify `RedirectUris` includes `http://localhost:4200/auth/callback`
3. Ensure exact match (no trailing slash differences)

### Issue 3: Invalid Client

**Symptom:**
```
invalid_client
```

**Solution:**
1. Verify client ID is `spa` in both frontend and IdentityServer
2. Check client is enabled in IdentityServer
3. Ensure `RequireClientSecret = false` for public client

### Issue 4: Token Not Sent to API

**Symptom:**
API returns 401 even when logged in

**Solution:**
1. Check `allowedUrls` in CoreModule OAuthModule config
2. Verify API URL matches allowed URLs
3. Check AuthInterceptor is registered
4. Verify token exists: `sessionStorage.getItem('access_token')`

### Issue 5: Silent Refresh Fails

**Symptom:**
```
silent refresh failed
```

**Solution:**
1. Check `/assets/silent-refresh.html` exists
2. Verify iframe is not blocked by browser
3. Check refresh token is present in sessionStorage
4. Verify IdentityServer supports refresh tokens for this client

### Issue 6: Discovery Document Load Fails

**Symptom:**
```
Error loading discovery document
```

**Solution:**
1. Verify IdentityServer is running
2. Check URL: `http://localhost:5001/.well-known/openid-configuration`
3. Check network connectivity
4. Verify CORS is configured

### Debugging Tips

1. **Enable debug mode:**
   ```typescript
   // In oauth.config.ts
   showDebugInformation: true
   ```

2. **Check OAuth events:**
   ```typescript
   // In AuthService
   this.oauthService.events.subscribe(e => {
     console.log('OAuth Event:', e);
   });
   ```

3. **Decode tokens:**
   - Copy access token from sessionStorage
   - Paste into https://jwt.io
   - Check claims and expiration

4. **Monitor network:**
   - Open DevTools Network tab
   - Filter by "token" or "authorize"
   - Check request/response details

5. **Check IdentityServer logs:**
   - Look for authentication failures
   - Check for CORS rejections
   - Verify token generation

---

## Future Enhancements

### 1. Backend for Frontend (BFF) Pattern

**Benefits:**
- Tokens stored server-side in HttpOnly cookies
- No token exposure to JavaScript (XSS protection)
- Server-side token refresh
- Better security for high-value applications

**Implementation:**
```
Frontend (Angular) <-> BFF (Node.js/ASP.NET) <-> APIs
                            |
                      IdentityServer
```

**Suggested Stack:**
- Node.js + Express + passport.js
- OR ASP.NET Core Proxy

### 2. Improved Login UX

**Current:** Separate login pages for each role
**Future:**
- Single login page
- "Login with Vendo" button
- No form fields (credentials entered on IdentityServer)
- Customized IdentityServer login UI per role

### 3. Multi-Factor Authentication (MFA)

- Add MFA support via IdentityServer
- Support for authenticator apps (TOTP)
- SMS verification
- Email verification

### 4. Social Login

- Add external identity providers:
  - Google
  - Microsoft
  - Facebook
  - GitHub

### 5. Remember Me / Persistent Login

- Option for persistent sessions
- Use secure cookies instead of sessionStorage
- Longer refresh token lifetime

### 6. Token Encryption

- Encrypt tokens before storing in sessionStorage
- Add additional layer of security
- Use Web Crypto API

### 7. Biometric Authentication

- Support for WebAuthn
- Fingerprint/Face ID on mobile
- Hardware security keys

### 8. Session Management

- Active sessions list
- Remote logout from all devices
- Session timeout warnings
- Concurrent session limits

### 9. Improved Error Handling

- User-friendly error messages
- Retry mechanisms
- Offline support
- Better logging and monitoring

### 10. Performance Optimization

- Lazy load OAuth modules
- Optimize token validation
- Cache discovery document
- Reduce bundle size

---

## Security Considerations

### Current Security Features

1. **PKCE**: Prevents authorization code interception
2. **State Parameter**: CSRF protection
3. **Nonce**: Replay attack prevention
4. **Token Validation**: Signature and expiration checks
5. **SessionStorage**: Automatic cleanup on tab close
6. **HTTPS Required**: In production (requireHttps: true)
7. **Token Refresh**: Short-lived access tokens
8. **Silent Refresh**: Background token renewal

### Security Best Practices Implemented

1. ✅ Use Authorization Code + PKCE flow
2. ✅ No client secrets in frontend
3. ✅ Short access token lifetime (1 hour)
4. ✅ Automatic token refresh
5. ✅ Secure token storage (sessionStorage)
6. ✅ Token validation on every request
7. ✅ CORS properly configured
8. ✅ HTTPS in production

### Security Recommendations for Production

1. **Enable HTTPS Everywhere**
   ```typescript
   requireHttps: true
   ```

2. **Implement BFF for sensitive data**
   - Store tokens server-side
   - Use HttpOnly cookies

3. **Add Content Security Policy (CSP)**
   ```html
   <meta http-equiv="Content-Security-Policy"
         content="default-src 'self'; script-src 'self'">
   ```

4. **Enable MFA for sensitive operations**

5. **Monitor and log authentication events**
   - Failed login attempts
   - Token refresh failures
   - Suspicious activity

6. **Regular security audits**
   - Dependency updates
   - Penetration testing
   - Code reviews

7. **Rate limiting**
   - Limit login attempts
   - Throttle token refresh requests

---

## Conclusion

The OAuth2/OIDC integration provides a modern, secure, and standards-based authentication system for the Vendo frontend application. The implementation uses industry best practices with Authorization Code + PKCE flow, automatic token refresh, and comprehensive error handling.

### Key Achievements

✅ **Secure**: PKCE prevents code interception attacks
✅ **Standard**: Fully compliant with OAuth 2.0 and OpenID Connect
✅ **Maintainable**: Well-documented and follows Angular best practices
✅ **Scalable**: Ready for multi-tenant and enterprise deployments
✅ **User-Friendly**: Seamless login experience with automatic token refresh

### Next Steps

1. **Test thoroughly** using the test scenarios in this document
2. **Monitor** authentication flows in development
3. **Consider BFF** for production deployment
4. **Add MFA** for enhanced security
5. **Customize** IdentityServer UI for better branding

For questions or issues, consult the troubleshooting section or refer to:
- [OAuth2 Migration Guide](/services/identity/OAUTH2-MIGRATION-GUIDE.md)
- [OAuth2 Endpoints Reference](/services/identity/OAUTH2-ENDPOINTS.md)
- [angular-oauth2-oidc Documentation](https://github.com/manfredsteyer/angular-oauth2-oidc)

---

**Document Version**: 1.0
**Last Updated**: 2025-10-22
**Author**: Frontend Engineering Team
