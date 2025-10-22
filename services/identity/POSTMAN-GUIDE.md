# Postman Collection Guide - Vendo Identity Service

This guide will help you test the Vendo Identity Service APIs using Postman.

## Files

- **Vendo-Identity-API.postman_collection.json** - The main collection with all API endpoints
- **Vendo-Identity.postman_environment.json** - Environment variables for local development

## Quick Setup

### 1. Import into Postman

1. Open Postman
2. Click **Import** button
3. Import both files:
   - `Vendo-Identity-API.postman_collection.json`
   - `Vendo-Identity.postman_environment.json`

### 2. Select Environment

1. In the top-right corner of Postman, select the environment dropdown
2. Choose **"Vendo Identity - Local Development"**

### 3. Update Environment Variables

Click the eye icon next to the environment dropdown to view/edit variables:

| Variable | Default Value | Description |
|----------|--------------|-------------|
| `baseUrl` | `https://localhost:7269` | Your Identity Server base URL |
| `client_id` | `client` | OAuth2 client ID (for Resource Owner Password flow) |
| `client_secret` | `secret` | OAuth2 client secret |
| `username` | `admin` | Test user username |
| `password` | `Admin@123` | Test user password |
| `access_token` | (auto-filled) | Automatically saved after authentication |
| `refresh_token` | (auto-filled) | Automatically saved when using offline_access |
| `service_access_token` | (auto-filled) | Token for service-to-service calls |
| `user_id` | (empty) | User ID for admin operations |

## Collection Structure

### 1. IdentityServer OAuth2/OIDC

Standard IdentityServer endpoints for OAuth2 and OpenID Connect flows.

#### Discovery Document
- **GET** `/.well-known/openid-configuration`
- Returns all available endpoints and configuration
- **Use this first** to verify IdentityServer is running

#### Token Endpoints

##### Token - Resource Owner Password
- **POST** `/connect/token`
- Grant Type: `password`
- Use when you have username/password credentials
- Automatically saves `access_token` and `refresh_token` to environment
- **Example Request:**
  ```
  grant_type=password
  client_id=client
  client_secret=secret
  username=admin
  password=Admin@123
  scope=openid profile email vendo.api.full_access roles tenant offline_access
  ```

##### Token - Client Credentials
- **POST** `/connect/token`
- Grant Type: `client_credentials`
- For service-to-service authentication (no user context)
- Automatically saves `service_access_token` to environment
- **Example Request:**
  ```
  grant_type=client_credentials
  client_id=service
  client_secret=service-secret
  scope=vendo.api.full_access
  ```

##### Token - Refresh Token
- **POST** `/connect/token`
- Grant Type: `refresh_token`
- Use to get a new access token without re-authenticating
- Requires a valid `refresh_token` in environment
- **Example Request:**
  ```
  grant_type=refresh_token
  client_id=client
  client_secret=secret
  refresh_token={{refresh_token}}
  ```

#### UserInfo
- **GET** `/connect/userinfo`
- Get authenticated user's claims (sub, name, email, roles, tenant_id, etc.)
- Requires: Bearer token in Authorization header
- Uses `{{access_token}}` from environment

#### Token Introspection
- **POST** `/connect/introspect`
- Check if a token is valid and get its metadata
- Used by resource servers to validate tokens
- Requires: Client credentials

#### Token Revocation
- **POST** `/connect/revocation`
- Revoke a token (for logout scenarios)
- Can revoke access tokens or refresh tokens

### 2. Account Management

Custom Identity API endpoints for user account operations.

#### Register User
- **POST** `/api/account/register`
- No authentication required
- **Request Body:**
  ```json
  {
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "SecureP@ssw0rd123",
    "firstName": "John",
    "lastName": "Doe"
  }
  ```

#### Login (DEPRECATED)
- **POST** `/api/account/login`
- ⚠️ **DEPRECATED:** Use `/connect/token` instead
- Maintained for backward compatibility only
- **Request Body:**
  ```json
  {
    "username": "admin",
    "password": "Admin@123",
    "role": "Customer"
  }
  ```

#### Get Profile
- **GET** `/api/account/profile`
- Get current user's profile
- Requires: Bearer token

#### Update Profile
- **PUT** `/api/account/profile`
- Update current user's profile
- Requires: Bearer token
- **Request Body:**
  ```json
  {
    "email": "updated.email@example.com",
    "firstName": "Jane",
    "lastName": "Smith"
  }
  ```

#### Change Password
- **POST** `/api/account/change-password`
- Change password for authenticated user
- Requires: Bearer token
- **Request Body:**
  ```json
  {
    "currentPassword": "OldP@ssw0rd123",
    "newPassword": "NewSecureP@ssw0rd123"
  }
  ```

#### Forgot Password
- **POST** `/api/account/forgot-password`
- Request password reset
- No authentication required
- **Request Body:**
  ```json
  {
    "email": "user@example.com"
  }
  ```

#### Reset Password
- **POST** `/api/account/reset-password`
- Reset password using token from email
- No authentication required
- **Request Body:**
  ```json
  {
    "email": "user@example.com",
    "token": "reset-token-from-email",
    "newPassword": "NewSecureP@ssw0rd123"
  }
  ```

### 3. User Management (Admin)

Administrative endpoints for managing users. **Requires Admin role.**

#### Get All Users
- **GET** `/api/users?isActive=true`
- List all users with optional filter
- Requires: Admin role + Bearer token

#### Get User by ID
- **GET** `/api/users/{{user_id}}`
- Get specific user details
- Requires: Admin role + Bearer token

#### Activate User
- **POST** `/api/users/{{user_id}}/activate`
- Activate a deactivated user account
- Requires: Admin role + Bearer token

#### Deactivate User
- **POST** `/api/users/{{user_id}}/deactivate`
- Deactivate a user account
- Requires: Admin role + Bearer token

## Common Workflows

### Workflow 1: Test OAuth2 Authentication (Recommended)

1. **Verify Server is Running**
   - Run: `Discovery Document`
   - Should return JSON with all endpoints

2. **Get Access Token**
   - Run: `Token - Resource Owner Password`
   - This automatically saves the `access_token` to your environment
   - Check the environment (eye icon) to see the saved token

3. **Get User Info**
   - Run: `UserInfo`
   - Uses the saved `access_token` automatically
   - Returns user claims

4. **Test Authenticated Endpoints**
   - Run: `Get Profile` to see your user data
   - All authenticated requests will use the saved `access_token`

### Workflow 2: Test User Registration and Management

1. **Register a New User**
   - Run: `Register User`
   - Update the request body with desired user details

2. **Login with New User**
   - Update environment variables `username` and `password`
   - Run: `Token - Resource Owner Password`

3. **Update Profile**
   - Run: `Update Profile` with new details

4. **Change Password**
   - Run: `Change Password`

### Workflow 3: Test Admin Operations

1. **Login as Admin**
   - Set environment: `username=admin`, `password=Admin@123`
   - Run: `Token - Resource Owner Password`

2. **List All Users**
   - Run: `Get All Users`

3. **Manage Specific User**
   - Copy a user ID from the list
   - Set environment variable: `user_id=<copied-id>`
   - Run: `Get User by ID`, `Activate User`, or `Deactivate User`

### Workflow 4: Test Token Refresh

1. **Get Initial Token with Offline Access**
   - Run: `Token - Resource Owner Password`
   - Ensure scope includes `offline_access`
   - Both `access_token` and `refresh_token` are saved

2. **Wait for Token to Expire** (or just test immediately)

3. **Refresh the Token**
   - Run: `Token - Refresh Token`
   - New `access_token` is automatically saved

4. **Verify New Token Works**
   - Run: `UserInfo` or `Get Profile`

### Workflow 5: Service-to-Service Authentication

1. **Get Service Token**
   - Run: `Token - Client Credentials`
   - This uses the `service` client (no user context)
   - Token saved as `service_access_token`

2. **Use Service Token for API Calls**
   - Manually change the `{{access_token}}` in requests to `{{service_access_token}}`
   - Or duplicate requests and change the token variable

## Available OAuth2 Clients

Your Identity Server has these pre-configured clients:

| Client ID | Grant Type | Client Secret | Purpose |
|-----------|-----------|---------------|---------|
| `client` | Resource Owner Password | `secret` | Legacy apps, backward compatibility |
| `service` | Client Credentials | `service-secret` | Service-to-service authentication |
| `swagger` | Authorization Code + PKCE | (none) | Swagger UI integration |
| `spa` | Authorization Code + PKCE | (none) | Single Page Applications |
| `interactive` | Authorization Code + PKCE | `secret` | Web applications |
| `mobile` | Authorization Code + PKCE | (none) | Mobile apps |
| `admin-portal` | Authorization Code + PKCE | (none) | Admin portal |
| `merchant-portal` | Authorization Code + PKCE | (none) | Merchant portal |

## Available Scopes

| Scope | Description |
|-------|-------------|
| `openid` | Required for OpenID Connect |
| `profile` | User profile claims (name, given_name, family_name) |
| `email` | Email claim |
| `vendo.api.full_access` | Full API access |
| `vendo.api.read` | Read-only API access |
| `vendo.api.write` | Write API access |
| `roles` | User role claims |
| `tenant` | Tenant information (tenant_id, tenant_name) |
| `offline_access` | Allows refresh tokens |

## Troubleshooting

### Issue: "SSL certificate problem"

**Solution:** Disable SSL verification in Postman:
1. Go to Settings (⚙️ icon)
2. Turn off "SSL certificate verification"
3. This is safe for local development with self-signed certificates

### Issue: "401 Unauthorized"

**Possible causes:**
1. Token expired - Run the token endpoint again to get a fresh token
2. Wrong token used - Make sure you're using `{{access_token}}` in Authorization header
3. Insufficient permissions - Admin endpoints require Admin role

### Issue: "400 Bad Request - invalid_grant"

**Possible causes:**
1. Wrong username/password
2. User account deactivated
3. Client credentials incorrect

### Issue: "Token not saving to environment"

**Solution:**
1. Make sure you've selected the environment (dropdown in top-right)
2. Check the "Tests" tab in the request - it should have a script that saves the token
3. Manually copy the token from response and paste into environment variables

## Test Users

Default test users (check your InMemoryUserRepository for actual users):

| Username | Password | Roles | Status |
|----------|----------|-------|--------|
| `admin` | `Admin@123` | Admin, Customer | Active |
| `merchant` | `Merchant@123` | Merchant | Active |
| `customer` | `Customer@123` | Customer | Active |

## Next Steps

1. Start your Identity Service: `dotnet run --project src/Api`
2. Import the Postman collection and environment
3. Run the "Discovery Document" request to verify setup
4. Follow Workflow 1 to test authentication
5. Explore other endpoints as needed

## Security Notes

⚠️ **For Development Only:**
- The client secrets (`secret`, `service-secret`) are hardcoded for development
- In production, use secure configuration (Azure Key Vault, environment variables, etc.)
- The Resource Owner Password flow is deprecated - use Authorization Code + PKCE for production

## Additional Resources

- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/identityserver/v6/)
- [OAuth 2.0 Grant Types](https://oauth.net/2/grant-types/)
- [OpenID Connect Specification](https://openid.net/connect/)
