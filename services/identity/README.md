# Vendo Identity Service

A complete Identity and Authentication service built with .NET 9.0, Duende IdentityServer, and Clean Architecture principles.

## Overview

This service provides comprehensive identity management and authentication capabilities for the Vendo platform, including:

- User registration and management
- Authentication and authorization using **OAuth2/OpenID Connect** standards
- Token-based API security with proper flows
- Multi-tenancy support (placeholder for full implementation)
- Role-based access control (RBAC)

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [README.md](./README.md) | This file - general overview and quick start |
| [QUICK-START.md](./QUICK-START.md) | Quick start guide for testing |
| [OAUTH2-MIGRATION-GUIDE.md](./OAUTH2-MIGRATION-GUIDE.md) | **Essential** - How to migrate from custom login to OAuth2/OIDC |
| [OAUTH2-ENDPOINTS.md](./OAUTH2-ENDPOINTS.md) | Complete OAuth2/OIDC endpoint reference |
| [IDENTITYSERVER-IMPLEMENTATION-SUMMARY.md](./IDENTITYSERVER-IMPLEMENTATION-SUMMARY.md) | Detailed implementation summary |
| [API-TESTING-GUIDE.md](./API-TESTING-GUIDE.md) | API testing guide with examples |
| [BACKEND-IMPLEMENTATION-SUMMARY.md](./BACKEND-IMPLEMENTATION-SUMMARY.md) | Backend implementation details |
| [../../07_SECURITY_AND_COMPLIANCE.md](../../07_SECURITY_AND_COMPLIANCE.md) | Security and compliance requirements |

## ⚠️ Important Notice

The custom `/api/account/login` endpoint is **deprecated**. All new applications should use standard OAuth2/OIDC flows via `/connect/token` and `/connect/authorize`. See [OAUTH2-MIGRATION-GUIDE.md](./OAUTH2-MIGRATION-GUIDE.md) for migration instructions.

## Technology Stack

- **.NET 9.0** - Runtime framework
- **Duende IdentityServer 7** - OAuth2/OpenID Connect server
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **AutoMapper** - Object-to-object mapping
- **BCrypt.Net** - Secure password hashing
- **Swashbuckle** - OpenAPI/Swagger documentation

## Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

```
Vendo.Identity/
├── Domain/                 # Enterprise business rules
│   ├── Entities/          # User aggregate
│   ├── ValueObjects/      # Email, Password with validation
│   ├── Events/            # Domain events
│   ├── Repositories/      # Repository interfaces
│   └── Exceptions/        # Domain exceptions
│
├── Application/           # Application business rules
│   ├── Commands/          # CQRS commands with handlers
│   ├── Queries/           # CQRS queries with handlers
│   ├── DTOs/              # Data transfer objects
│   ├── Validators/        # FluentValidation validators
│   ├── Mappings/          # AutoMapper profiles
│   └── Common/            # Shared interfaces and models
│
├── Infrastructure/        # External concerns
│   ├── Persistence/       # In-memory repository implementation
│   ├── Identity/          # IdentityServer configuration
│   └── Services/          # Password hashing, etc.
│
└── Api/                   # Presentation layer
    ├── Controllers/       # REST API endpoints
    ├── Middleware/        # Error handling, etc.
    └── Models/            # API request/response models
```

## Features

### User Management

1. **User Registration**
   - Username validation (alphanumeric, hyphens, underscores)
   - Email validation and uniqueness check
   - Strong password requirements (min 8 chars, uppercase, lowercase, digit, special char)
   - Role assignment

2. **Authentication**
   - Username/password authentication
   - JWT token generation via IdentityServer
   - OAuth2 Resource Owner Password flow
   - OAuth2 Authorization Code flow with PKCE

3. **Profile Management**
   - Get user profile
   - Update user information
   - Change password with current password verification

4. **User Administration** (Admin only)
   - List all users with optional filtering
   - View specific user details
   - Activate/deactivate user accounts

### Security Features

- **Password Security**: BCrypt hashing with salt
- **JWT Authentication**: Bearer token validation
- **Role-Based Authorization**: Admin and User roles
- **CORS Configuration**: Configurable origins
- **HTTPS Enforcement**: Secure communication
- **Domain Validation**: Strong typing with value objects

## Pre-configured Test Users

The service comes with pre-seeded test users:

| Username  | Email                 | Password      | Roles       |
|-----------|-----------------------|---------------|-------------|
| admin     | admin@vendo.com       | Admin@123     | Admin, User |
| testuser  | user@vendo.com        | User@123      | User        |
| johndoe   | john.doe@vendo.com    | JohnDoe@123   | User        |

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Your favorite IDE (Visual Studio 2022, VS Code, or Rider)

### Running the Service

1. **Clone and navigate to the project:**
   ```bash
   cd services/identity
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the API:**
   ```bash
   cd src/Api
   dotnet run
   ```

5. **Access Swagger UI:**
   ```
   https://localhost:5001/swagger
   ```

### API Endpoints

#### Account Endpoints (Public/Authenticated)

- `POST /api/account/register` - Register a new user
- `POST /api/account/login` - Authenticate user
- `GET /api/account/profile` - Get current user profile (Authenticated)
- `PUT /api/account/profile` - Update profile (Authenticated)
- `POST /api/account/change-password` - Change password (Authenticated)

#### Users Endpoints (Admin Only)

- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users/{id}/activate` - Activate user
- `POST /api/users/{id}/deactivate` - Deactivate user

#### IdentityServer Endpoints

- `GET /.well-known/openid-configuration` - OpenID Connect discovery
- `POST /connect/token` - Token endpoint
- `POST /connect/authorize` - Authorization endpoint
- `GET /connect/userinfo` - User info endpoint

#### Health Check

- `GET /health` - Service health status

## Authentication with OAuth2/OIDC

### Important: Custom Login Endpoint Deprecated

The custom `/api/account/login` endpoint is **deprecated** and will be removed in a future version. Please migrate to standard OAuth2/OIDC flows. See [OAUTH2-MIGRATION-GUIDE.md](./OAUTH2-MIGRATION-GUIDE.md) for detailed instructions.

### Recommended: OAuth2 Authorization Code Flow with PKCE

This is the industry-standard, most secure authentication flow for web and mobile applications.

#### Using Swagger UI (Easiest for Testing)

1. Navigate to `https://localhost:5001/swagger`
2. Click the **"Authorize"** button (top right)
3. Select all scopes you need
4. Click **"Authorize"**
5. You'll be redirected to the IdentityServer login page
6. Login with test credentials (e.g., `admin` / `Admin@123`)
7. After successful authentication, you can test all protected endpoints

#### Using OAuth2 Libraries in Your Application

**Angular:**
```bash
npm install angular-oauth2-oidc
```

**React:**
```bash
npm install oidc-client-ts
```

See [OAUTH2-MIGRATION-GUIDE.md](./OAUTH2-MIGRATION-GUIDE.md) for complete implementation examples.

### Alternative: Resource Owner Password Flow (Legacy)

For testing or legacy applications only. **Not recommended** for new applications.

```bash
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin@123&client_id=client&client_secret=secret&scope=openid profile email vendo.api.full_access roles tenant"
```

### Service-to-Service: Client Credentials Flow

For backend services and microservices:

```bash
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=service&client_secret=service-secret&scope=vendo.api.full_access"
```

### Direct API Testing (No OAuth2)

These endpoints don't require OAuth2 authentication:

1. **Register a new user:**
```bash
curl -X POST "https://localhost:5001/api/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "NewUser@123",
    "firstName": "New",
    "lastName": "User"
  }'
```

2. **Login (Deprecated - Use /connect/token instead):**
```bash
curl -X POST "https://localhost:5001/api/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

## Configuration

### IdentityServer Clients

The service is pre-configured with **8 clients** for different use cases:

1. **Swagger UI** (`swagger`)
   - **Flow**: Authorization Code with PKCE
   - **Secret**: None (public client)
   - **Use**: API testing via Swagger UI

2. **Single Page Application** (`spa`)
   - **Flow**: Authorization Code with PKCE
   - **Secret**: None (public client)
   - **Use**: Angular, React, Vue applications
   - **Ports**: localhost:4200 (HTTP/HTTPS)

3. **Mobile Application** (`mobile`)
   - **Flow**: Authorization Code with PKCE
   - **Secret**: None (public client)
   - **Use**: iOS, Android, React Native apps
   - **Redirect**: Custom URL schemes (com.vendo.app://)

4. **Interactive Web Application** (`interactive`)
   - **Flow**: Authorization Code with PKCE
   - **Secret**: `secret` (confidential client)
   - **Use**: Server-side web applications
   - **Ports**: localhost:5002, localhost:4200

5. **Admin Portal** (`admin-portal`)
   - **Flow**: Authorization Code with PKCE
   - **Secret**: None (public client)
   - **Use**: Admin dashboard
   - **Ports**: localhost:4300 (HTTP/HTTPS)
   - **Security**: Shorter token lifetime (30 min)

6. **Merchant Portal** (`merchant-portal`)
   - **Flow**: Authorization Code with PKCE
   - **Secret**: None (public client)
   - **Use**: Merchant dashboard
   - **Ports**: localhost:4400 (HTTP/HTTPS)

7. **Legacy Client** (`client`) - **DEPRECATED**
   - **Flow**: Resource Owner Password Credentials
   - **Secret**: `secret`
   - **Use**: Legacy applications during migration
   - **Note**: Migrate to Authorization Code + PKCE

8. **Backend Service** (`service`)
   - **Flow**: Client Credentials
   - **Secret**: `service-secret`
   - **Use**: Service-to-service authentication
   - **No user context**: For background jobs, microservices

### API Scopes

**Identity Scopes** (user information):
- `openid` - OpenID Connect (required for OIDC)
- `profile` - User profile information (name, given_name, family_name)
- `email` - User email address
- `roles` - User roles for authorization
- `tenant` - Tenant information (tenant_id, tenant_name)

**API Scopes** (resource access):
- `vendo.api.full_access` - Full API access (read + write)
- `vendo.api.read` - Read-only access
- `vendo.api.write` - Write-only access

**Recommended Scope Combinations**:
- **Web/Mobile Apps**: `openid profile email vendo.api.full_access roles tenant`
- **Admin Portal**: `openid profile email vendo.api.full_access roles tenant`
- **Merchant Portal**: `openid profile email vendo.api.read vendo.api.write roles tenant`
- **Backend Service**: `vendo.api.full_access` (no user scopes)

## Development Notes

### In-Memory Storage

The current implementation uses in-memory storage for:
- User data
- IdentityServer configuration
- Test data

**Note:** All data is lost when the service restarts. For production, implement persistent storage (Entity Framework Core with SQL Server/PostgreSQL).

### Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (!@#$%^&*(),.?"':{}|<>)

### Adding New Users

Use the registration endpoint or add them in `Infrastructure/Persistence/SeedData.cs`.

## Production Considerations

Before deploying to production:

1. **Replace Developer Signing Credentials**
   - Generate a proper X.509 certificate
   - Update `.AddDeveloperSigningCredential()` to `.AddSigningCredential(certificate)`

2. **Implement Persistent Storage**
   - Add Entity Framework Core
   - Replace `InMemoryUserRepository` with database implementation
   - Configure IdentityServer stores for persistence

3. **Configure HTTPS Properly**
   - Set `RequireHttpsMetadata = true`
   - Configure valid SSL certificates

4. **Environment Configuration**
   - Move secrets to Azure Key Vault or similar
   - Configure proper CORS origins
   - Set appropriate token lifetimes

5. **Add Logging and Monitoring**
   - Integrate Application Insights or similar
   - Configure Serilog for structured logging
   - Set up health checks for monitoring

6. **Security Hardening**
   - Implement rate limiting
   - Add CAPTCHA for registration
   - Configure account lockout policies
   - Implement email verification
   - Add two-factor authentication

## Project Structure Details

### Domain Layer
Pure business logic with no external dependencies. Contains:
- **User Entity**: Aggregate root with domain logic
- **Value Objects**: Email and Password with built-in validation
- **Domain Events**: UserRegistered, UserUpdated, PasswordChanged
- **Repository Interfaces**: Contract for data access

### Application Layer
Orchestrates business workflows. Contains:
- **Commands**: RegisterUser, UpdateUser, ChangePassword, Activate/DeactivateUser
- **Queries**: GetUser, GetUsers, AuthenticateUser
- **Handlers**: MediatR handlers for commands and queries
- **Validators**: FluentValidation rules
- **DTOs**: Data transfer objects for API communication

### Infrastructure Layer
Implements external concerns. Contains:
- **Repositories**: In-memory implementation of IUserRepository
- **IdentityServer**: Configuration for clients, resources, scopes
- **Profile Service**: Custom user claims inclusion
- **Password Validator**: BCrypt-based password hashing

### API Layer
HTTP interface. Contains:
- **Controllers**: AccountController (public), UsersController (admin)
- **Middleware**: Global exception handling
- **Models**: API request/response models

## Troubleshooting

### Common Issues

1. **Port already in use**
   - Change port in `Properties/launchSettings.json`

2. **IdentityServer discovery fails**
   - Ensure the authority URL matches the service URL
   - Check HTTPS configuration

3. **JWT validation fails**
   - Verify the `Authority` setting in JWT Bearer configuration
   - Check token expiration

4. **Swagger OAuth2 not working**
   - Ensure redirect URI matches: `https://localhost:5001/swagger/oauth2-redirect.html`
   - Check client configuration in IdentityServerConfig

## Contributing

Follow the established patterns:
- Use CQRS for new features
- Add validators for all commands
- Update Swagger documentation
- Follow Clean Architecture boundaries
- Write unit tests (when test project is added)

## License

Copyright © 2025 Vendo Platform. All rights reserved.

## Support

For issues or questions, contact the Vendo development team.
