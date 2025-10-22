# Vendo Identity Service

A complete Identity and Authentication service built with .NET 9.0, Duende IdentityServer, and Clean Architecture principles.

## Overview

This service provides comprehensive identity management and authentication capabilities for the Vendo platform, including:

- User registration and management
- Authentication and authorization
- JWT token-based API security
- OAuth2/OpenID Connect flows
- Role-based access control (RBAC)

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

## Testing with Swagger

### Option 1: OAuth2 Authorization Code Flow (Recommended)

1. Click the "Authorize" button in Swagger UI
2. Select all scopes
3. Click "Authorize"
4. You'll be redirected to the IdentityServer login page
5. Login with test credentials
6. After successful authentication, you can test protected endpoints

### Option 2: Resource Owner Password Flow

1. Get a token using cURL or Postman:

```bash
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin@123&client_id=client&client_secret=secret&scope=openid profile email vendo.api.full_access roles"
```

2. Copy the `access_token` from the response
3. Click "Authorize" in Swagger and paste the token

### Option 3: Direct API Testing

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

2. **Login:**
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

The service is pre-configured with three clients:

1. **Swagger UI** (`swagger`)
   - Authorization Code with PKCE
   - No client secret required
   - For testing via Swagger UI

2. **Resource Owner Password** (`client`)
   - Username/password flow
   - Secret: `secret`
   - For programmatic access

3. **Interactive Application** (`interactive`)
   - Authorization Code with PKCE
   - Secret: `secret`
   - For web applications

### API Scopes

- `openid` - OpenID Connect
- `profile` - User profile information
- `email` - User email
- `vendo.api.full_access` - Full API access
- `vendo.api.read` - Read-only access
- `vendo.api.write` - Write access
- `roles` - User roles

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
