# Vendo Identity Service - Quick Start Guide

## Start the Service

```bash
cd services/identity/src/Api
dotnet run
```

The service will start at: `https://localhost:5001`

## Access Points

- **Swagger UI**: https://localhost:5001/swagger
- **IdentityServer Discovery**: https://localhost:5001/.well-known/openid-configuration
- **Health Check**: https://localhost:5001/health

## Test Users

| Username | Password    | Role  |
|----------|-------------|-------|
| admin    | Admin@123   | Admin |
| testuser | User@123    | User  |
| johndoe  | JohnDoe@123 | User  |

## Quick API Tests

### 1. Register a New User

```bash
curl -X POST https://localhost:5001/api/account/register \
  -H "Content-Type: application/json" \
  -k -d '{
    "username": "myuser",
    "email": "myuser@example.com",
    "password": "MyPass@123",
    "firstName": "My",
    "lastName": "User"
  }'
```

### 2. Login (Test Authentication)

```bash
curl -X POST https://localhost:5001/api/account/login \
  -H "Content-Type: application/json" \
  -k -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

### 3. Get OAuth2 Token

```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -k -d "grant_type=password&username=admin&password=Admin@123&client_id=client&client_secret=secret&scope=openid profile email vendo.api.full_access roles"
```

**Response:**
```json
{
  "access_token": "eyJhbGc...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "scope": "openid profile email vendo.api.full_access roles"
}
```

### 4. Use Token to Access Protected Endpoint

```bash
# Replace YOUR_TOKEN with the access_token from step 3
curl -X GET https://localhost:5001/api/account/profile \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -k
```

### 5. List All Users (Admin Only)

```bash
# Use admin token
curl -X GET https://localhost:5001/api/users \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -k
```

## Using Swagger UI

1. Open https://localhost:5001/swagger
2. Click **"Authorize"** button (top right)
3. Select all scopes
4. Click **"Authorize"**
5. A login window will open (you may need to trust the certificate)
6. Login with: `admin` / `Admin@123`
7. After authentication, you can test all endpoints

## Testing Workflow

### User Registration Flow
1. Register new user → `POST /api/account/register`
2. Login with credentials → `POST /api/account/login`
3. Get profile → `GET /api/account/profile` (requires token)
4. Update profile → `PUT /api/account/profile` (requires token)
5. Change password → `POST /api/account/change-password` (requires token)

### Admin Management Flow
1. Get OAuth2 token with admin credentials
2. List all users → `GET /api/users`
3. Get specific user → `GET /api/users/{id}`
4. Deactivate user → `POST /api/users/{id}/deactivate`
5. Activate user → `POST /api/users/{id}/activate`

## Common Response Formats

### Success Response
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "username": "admin",
    "email": "admin@vendo.com",
    ...
  },
  "error": null,
  "validationErrors": null
}
```

### Error Response
```json
{
  "success": false,
  "data": null,
  "error": "Error message here",
  "validationErrors": ["Validation error 1", "Validation error 2"]
}
```

## Available OAuth2 Clients

### 1. Swagger Client
- **Client ID**: `swagger`
- **Grant Type**: Authorization Code + PKCE
- **No Secret Required**
- **Use for**: Testing in Swagger UI

### 2. Password Client
- **Client ID**: `client`
- **Client Secret**: `secret`
- **Grant Type**: Resource Owner Password
- **Use for**: Direct API access, mobile apps, testing

### 3. Interactive Client
- **Client ID**: `interactive`
- **Client Secret**: `secret`
- **Grant Type**: Authorization Code + PKCE
- **Use for**: Web applications

## Available Scopes

- `openid` - Required for OpenID Connect
- `profile` - User profile (name, etc.)
- `email` - User email address
- `roles` - User roles
- `vendo.api.full_access` - Full API access
- `vendo.api.read` - Read-only API access
- `vendo.api.write` - Write-only API access

## Password Requirements

When registering or changing passwords:
- Minimum 8 characters
- At least 1 uppercase letter (A-Z)
- At least 1 lowercase letter (a-z)
- At least 1 digit (0-9)
- At least 1 special character (!@#$%^&*(),.?"':{}|<>)

Example valid passwords:
- `MyPass@123`
- `Admin@123`
- `SecureP@ssw0rd`

## Troubleshooting

### Certificate Trust Issues
If you get SSL certificate errors:

**Windows:**
```bash
# Trust the development certificate
dotnet dev-certs https --trust
```

**Linux/Mac:**
```bash
# Export and manually trust the certificate
dotnet dev-certs https -ep ~/.aspnet/https/cert.pem --format PEM
```

Or use `-k` flag in curl to skip certificate validation (development only)

### Port Already in Use
Change the port in `Properties/launchSettings.json`:
```json
{
  "https": {
    "applicationUrl": "https://localhost:5001;http://localhost:5000"
  }
}
```

### Can't Access Swagger
1. Ensure the service is running
2. Check the URL: https://localhost:5001/swagger (note the 's' in https)
3. Accept the browser security warning about the development certificate

## Next Steps

1. **Test all endpoints** using Swagger UI
2. **Try the OAuth2 flows** with different clients
3. **Integrate with your frontend** application
4. **Review the code** to understand the architecture
5. **Customize** for your specific requirements

## Support

- Check the main README.md for detailed documentation
- Review the code comments for implementation details
- Examine the IdentityServer logs for authentication issues

## Quick Architecture Overview

```
┌─────────────────────────────────────────────────┐
│               API Layer (Controllers)            │
│  - Account Controller (Public/Authenticated)     │
│  - Users Controller (Admin Only)                 │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│          Application Layer (CQRS)                │
│  - Commands (RegisterUser, UpdateUser, etc.)     │
│  - Queries (GetUser, AuthenticateUser, etc.)     │
│  - Validators (FluentValidation)                 │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│              Domain Layer                        │
│  - User Entity                                   │
│  - Value Objects (Email, Password)               │
│  - Domain Events                                 │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│         Infrastructure Layer                     │
│  - In-Memory Repository                          │
│  - Duende IdentityServer Configuration           │
│  - BCrypt Password Hasher                        │
└──────────────────────────────────────────────────┘
```

Happy coding!
