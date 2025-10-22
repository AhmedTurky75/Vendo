# Quick Reference - Backend Implementation

## What Was Implemented

### 1. Enhanced Login with Role-Based Authentication
- **Feature**: Login endpoint now accepts optional `role` parameter
- **File**: `/home/user/Vendo/services/identity/src/Api/Controllers/AccountController.cs`
- **Request Example**:
  ```json
  {
    "username": "admin",
    "password": "Admin@123",
    "role": "Admin"
  }
  ```

### 2. JWT Token Generation
- **Feature**: Login returns JWT access and refresh tokens
- **Files**:
  - Interface: `src/Application/Common/Interfaces/IJwtTokenService.cs`
  - Implementation: `src/Infrastructure/Services/JwtTokenService.cs`
- **Response Includes**:
  - `accessToken`: JWT with user claims and roles
  - `refreshToken`: For future refresh token flow
  - `expiresIn`: Token expiration in seconds

### 3. Forgot Password
- **Endpoint**: `POST /api/account/forgot-password`
- **Files**:
  - Command: `src/Application/Commands/ForgotPassword/ForgotPasswordCommand.cs`
  - Handler: `src/Application/Commands/ForgotPassword/ForgotPasswordCommandHandler.cs`
  - Validator: `src/Application/Commands/ForgotPassword/ForgotPasswordCommandValidator.cs`
- **Features**:
  - Generates secure 60-minute tokens
  - Logs token to console (dev mode)
  - Ready for email integration (TODO comment added)
  - Prevents email enumeration (always returns success)

### 4. Reset Password
- **Endpoint**: `POST /api/account/reset-password`
- **Files**:
  - Command: `src/Application/Commands/ResetPassword/ResetPasswordCommand.cs`
  - Handler: `src/Application/Commands/ResetPassword/ResetPasswordCommandHandler.cs`
  - Validator: `src/Application/Commands/ResetPassword/ResetPasswordCommandValidator.cs`
- **Features**:
  - Validates token and expiration
  - Single-use tokens (invalidated after reset)
  - Password strength validation

## Quick Start Testing

### 1. Start the Service
```bash
cd /home/user/Vendo/services/identity
dotnet run --project src/Api
```

### 2. Test Login (Basic)
```bash
curl -X POST http://localhost:5001/api/account/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

### 3. Test Login (with Role)
```bash
curl -X POST http://localhost:5001/api/account/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "merchant",
    "password": "Merchant@123",
    "role": "Merchant"
  }'
```

### 4. Test Forgot Password
```bash
curl -X POST http://localhost:5001/api/account/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@vendo.com"}'
```
**Check console logs for the reset token!**

### 5. Test Reset Password
```bash
curl -X POST http://localhost:5001/api/account/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@vendo.com",
    "token": "TOKEN_FROM_CONSOLE",
    "newPassword": "NewAdmin@123"
  }'
```

## Test Users

| Role     | Username | Password      |
|----------|----------|---------------|
| Admin    | admin    | Admin@123     |
| Merchant | merchant | Merchant@123  |
| Customer | customer | Customer@123  |

## Key Files Modified

1. **AccountController.cs** - Added forgot/reset password endpoints
2. **User.cs** - Added password reset token methods
3. **AuthenticationResultDto.cs** - Added token fields
4. **AuthenticateUserQueryHandler.cs** - Added JWT generation
5. **DependencyInjection.cs** - Registered JWT service
6. **SeedData.cs** - Added merchant and customer users
7. **appsettings.json** - Added JWT configuration

## Key Files Created

1. **IJwtTokenService.cs** - JWT service interface
2. **JwtTokenService.cs** - JWT implementation
3. **PasswordResetToken.cs** - Value object for tokens
4. **ForgotPasswordCommand** - Command + Handler + Validator
5. **ResetPasswordCommand** - Command + Handler + Validator
6. **ForgotPasswordRequest.cs** - API request model
7. **ResetPasswordRequest.cs** - API request model

## Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Secret": "VendoIdentitySecretKeyForDevelopmentMinimum32Characters!",
    "Issuer": "https://localhost:5001",
    "Audience": "vendo.api",
    "ExpirationMinutes": 60
  }
}
```

## Production TODO List

1. **Email Integration** - Implement email service for password reset
2. **Rate Limiting** - Add rate limiting to prevent abuse
3. **JWT Secret** - Move to secure configuration (Key Vault)
4. **Refresh Token Storage** - Implement token refresh flow
5. **Token Revocation** - Add logout with token blacklisting

## Documentation

- **API-TESTING-GUIDE.md** - Complete testing guide with curl examples
- **BACKEND-IMPLEMENTATION-SUMMARY.md** - Detailed implementation summary
- **QUICK-REFERENCE.md** - This file

## Swagger UI

Access interactive API documentation at:
```
http://localhost:5001/swagger
```

## Common Issues & Solutions

### Issue: Token not found in console
**Solution**: Check the logs with appropriate log level (Debug for development)

### Issue: Token expired
**Solution**: Tokens expire after 60 minutes. Request a new one.

### Issue: Role authentication fails
**Solution**: Ensure the user has the requested role. Check User.Roles property.

### Issue: JWT validation fails
**Solution**: Check JWT configuration in appsettings.json matches between services.

## Architecture Patterns Used

- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ CQRS with MediatR
- ✅ Repository Pattern
- ✅ Value Objects (Email, Password, PasswordResetToken)
- ✅ Result Pattern for error handling
- ✅ FluentValidation for input validation
- ✅ Dependency Injection
- ✅ Domain Events

## Security Features

- ✅ BCrypt password hashing
- ✅ JWT tokens with claims
- ✅ Token expiration
- ✅ Single-use reset tokens
- ✅ Email enumeration prevention
- ✅ Role-based access control
- ✅ Secure token generation (RandomNumberGenerator)

## Next Steps

1. **Frontend Integration**: Update Angular login components to use new API
2. **Testing**: Run comprehensive tests on all endpoints
3. **Production**: Implement TODO items (email, rate limiting, etc.)
4. **Deployment**: Deploy to staging for integration testing
5. **Monitoring**: Set up logging and monitoring for production

## Support

For detailed information, see:
- API-TESTING-GUIDE.md
- BACKEND-IMPLEMENTATION-SUMMARY.md
- README.md
