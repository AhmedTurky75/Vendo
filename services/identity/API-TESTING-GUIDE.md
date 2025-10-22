# Identity Service API Testing Guide

## Overview
This guide provides instructions for testing the Identity service API endpoints, including login, forgot password, and reset password functionality.

## Base URL
```
http://localhost:5001/api
```

## Test Users

The following test users are seeded automatically on startup:

| Username  | Email                  | Password       | Roles                |
|-----------|------------------------|----------------|----------------------|
| admin     | admin@vendo.com        | Admin@123      | Admin, User          |
| merchant  | merchant@vendo.com     | Merchant@123   | Merchant, User       |
| customer  | customer@vendo.com     | Customer@123   | Customer, User       |
| testuser  | user@vendo.com         | User@123       | User                 |
| johndoe   | john.doe@vendo.com     | JohnDoe@123    | User                 |

## API Endpoints

### 1. Register User

**Endpoint:** `POST /api/account/register`

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

**Success Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "username": "newuser",
    "email": "newuser@vendo.com",
    "firstName": "New",
    "lastName": "User",
    "isActive": true,
    "roles": ["User"],
    "createdAt": "2025-10-22T00:00:00Z",
    "updatedAt": "2025-10-22T00:00:00Z"
  },
  "error": null,
  "validationErrors": null
}
```

### 2. Login (Basic - No Role)

**Endpoint:** `POST /api/account/login`

**Request Body:**
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "isAuthenticated": true,
    "user": {
      "id": "guid",
      "username": "admin",
      "email": "admin@vendo.com",
      "firstName": "System",
      "lastName": "Administrator",
      "isActive": true,
      "roles": ["Admin", "User"],
      "createdAt": "2025-10-22T00:00:00Z",
      "updatedAt": "2025-10-22T00:00:00Z"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token",
    "expiresIn": 3600,
    "message": "Authentication successful"
  },
  "error": null,
  "validationErrors": null
}
```

### 3. Login with Role Verification (Admin)

**Endpoint:** `POST /api/account/login`

**Request Body:**
```json
{
  "username": "admin",
  "password": "Admin@123",
  "role": "Admin"
}
```

**Success Response:** Same as basic login

**Failed Response (User doesn't have role):**
```json
{
  "success": true,
  "data": {
    "isAuthenticated": false,
    "user": null,
    "accessToken": null,
    "refreshToken": null,
    "expiresIn": null,
    "message": "User does not have the required role: Admin"
  },
  "error": null,
  "validationErrors": null
}
```

### 4. Login with Role Verification (Merchant)

**Endpoint:** `POST /api/account/login`

**Request Body:**
```json
{
  "username": "merchant",
  "password": "Merchant@123",
  "role": "Merchant"
}
```

### 5. Login with Role Verification (Customer)

**Endpoint:** `POST /api/account/login`

**Request Body:**
```json
{
  "username": "customer",
  "password": "Customer@123",
  "role": "Customer"
}
```

### 6. Forgot Password

**Endpoint:** `POST /api/account/forgot-password`

**Request Body:**
```json
{
  "email": "admin@vendo.com"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "If the email exists, a password reset link has been sent. Please check your email."
  },
  "error": null,
  "validationErrors": null
}
```

**Note:** In development mode, the reset token will be logged to the console. Look for a log entry like:
```
Password reset token generated for user {UserId}. Token: {Token}, Expires at: {ExpiresAt}
DEV MODE - Password Reset URL: http://localhost:4200/reset-password?token={Token}&email={Email}
```

### 7. Reset Password

**Endpoint:** `POST /api/account/reset-password`

**Request Body:**
```json
{
  "email": "admin@vendo.com",
  "token": "token-from-forgot-password-response",
  "newPassword": "NewAdmin@123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "message": "Password has been reset successfully"
  },
  "error": null,
  "validationErrors": null
}
```

**Error Response (400 Bad Request - Invalid/Expired Token):**
```json
{
  "success": false,
  "data": null,
  "error": "Invalid or expired reset token",
  "validationErrors": null
}
```

### 8. Get Profile (Authenticated)

**Endpoint:** `GET /api/account/profile`

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "username": "admin",
    "email": "admin@vendo.com",
    "firstName": "System",
    "lastName": "Administrator",
    "isActive": true,
    "roles": ["Admin", "User"],
    "createdAt": "2025-10-22T00:00:00Z",
    "updatedAt": "2025-10-22T00:00:00Z"
  },
  "error": null,
  "validationErrors": null
}
```

### 9. Update Profile (Authenticated)

**Endpoint:** `PUT /api/account/profile`

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "email": "newemail@vendo.com",
  "firstName": "Updated",
  "lastName": "Name"
}
```

### 10. Change Password (Authenticated)

**Endpoint:** `POST /api/account/change-password`

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:**
```json
{
  "currentPassword": "Admin@123",
  "newPassword": "NewAdmin@456"
}
```

## Testing Scenarios

### Scenario 1: Complete Password Reset Flow

1. **Request Password Reset:**
   ```bash
   curl -X POST http://localhost:5001/api/account/forgot-password \
     -H "Content-Type: application/json" \
     -d '{"email": "admin@vendo.com"}'
   ```

2. **Check Server Console for Token:**
   Look for the log message containing the reset token.

3. **Reset Password:**
   ```bash
   curl -X POST http://localhost:5001/api/account/reset-password \
     -H "Content-Type: application/json" \
     -d '{
       "email": "admin@vendo.com",
       "token": "TOKEN_FROM_CONSOLE",
       "newPassword": "NewAdmin@123"
     }'
   ```

4. **Login with New Password:**
   ```bash
   curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "admin",
       "password": "NewAdmin@123"
     }'
   ```

### Scenario 2: Role-Based Login

1. **Admin Login:**
   ```bash
   curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "admin",
       "password": "Admin@123",
       "role": "Admin"
     }'
   ```

2. **Merchant Login:**
   ```bash
   curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "merchant",
       "password": "Merchant@123",
       "role": "Merchant"
     }'
   ```

3. **Customer Login:**
   ```bash
   curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "customer",
       "password": "Customer@123",
       "role": "Customer"
     }'
   ```

4. **Failed Role Login (User trying to login as Admin):**
   ```bash
   curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{
       "username": "testuser",
       "password": "User@123",
       "role": "Admin"
     }'
   ```

### Scenario 3: JWT Token Usage

1. **Login and Get Token:**
   ```bash
   RESPONSE=$(curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{"username": "admin", "password": "Admin@123"}')

   TOKEN=$(echo $RESPONSE | jq -r '.data.accessToken')
   ```

2. **Use Token to Get Profile:**
   ```bash
   curl -X GET http://localhost:5001/api/account/profile \
     -H "Authorization: Bearer $TOKEN"
   ```

3. **Use Token to Update Profile:**
   ```bash
   curl -X PUT http://localhost:5001/api/account/profile \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
       "email": "admin@vendo.com",
       "firstName": "System",
       "lastName": "Admin"
     }'
   ```

## Error Cases to Test

### 1. Invalid Credentials
```json
{
  "username": "admin",
  "password": "WrongPassword"
}
```

### 2. Non-existent User
```json
{
  "username": "nonexistent",
  "password": "Password@123"
}
```

### 3. Invalid Email Format
```json
{
  "email": "invalidemail"
}
```

### 4. Expired Token
Reset password with a token that has expired (>60 minutes old).

### 5. Reused Token
Try to reset password twice with the same token.

### 6. Wrong Role
Login as a user without the requested role.

## Swagger UI

The API includes Swagger UI for interactive testing:

**URL:** `http://localhost:5001/swagger`

Use Swagger UI to:
- View all available endpoints
- Test endpoints interactively
- View request/response schemas
- Download OpenAPI specification

## Postman Collection

A Postman collection can be created with all the above requests for easier testing. Import the following endpoints into Postman:

1. Register User
2. Login (Basic)
3. Login (with Role)
4. Forgot Password
5. Reset Password
6. Get Profile
7. Update Profile
8. Change Password

## Notes

- All timestamps are in UTC
- JWT tokens expire after 60 minutes by default (configurable in appsettings.json)
- Password reset tokens expire after 60 minutes
- Password reset tokens can only be used once
- The forgot password endpoint always returns success (for security)
- Role parameter is optional in login request
- If role is not specified, login succeeds if credentials are valid
- If role is specified, login succeeds only if user has that role
