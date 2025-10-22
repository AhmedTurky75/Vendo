# Auth Feature Module

This module contains all authentication-related components and functionality for the Vendo platform.

## Components

### 1. Admin Login Component
- **Path**: `/login/admin`
- **Purpose**: Login interface for platform administrators
- **Features**:
  - Email/password authentication
  - Form validation
  - Loading states
  - Error handling
  - Forgot password link
  - Links to other login types
- **Styling**: Purple gradient theme

### 2. Merchant Login Component
- **Path**: `/login/merchant`
- **Purpose**: Login interface for store merchants/vendors
- **Features**:
  - Email/password authentication
  - "Remember me" functionality
  - Form validation
  - Loading states
  - Error handling
  - Forgot password link
  - Link to merchant registration
  - Links to other login types
- **Styling**: Emerald/Teal gradient theme

### 3. Customer Login Component
- **Path**: `/login/customer`
- **Purpose**: Login interface for shoppers/customers
- **Features**:
  - Email/password authentication
  - "Remember me" functionality
  - Social login options (UI placeholders for GitHub and Facebook)
  - Form validation
  - Loading states
  - Error handling
  - Forgot password link
  - Link to customer registration
  - Links to other login types
- **Styling**: Blue gradient theme

### 4. Forgot Password Component
- **Path**: `/forgot-password`
- **Purpose**: Request password reset link
- **Features**:
  - Email input with validation
  - Form validation (email format)
  - Success/error message display
  - API integration with backend
  - Back to login navigation
  - Loading states during submission
- **Styling**: Indigo gradient theme
- **API Endpoint**: `POST /api/account/forgot-password`
- **Request**: `{ email: string }`
- **Response**: `{ message: string }`

### 5. Reset Password Component
- **Path**: `/reset-password?token=xxx&email=yyy`
- **Purpose**: Reset password using token from email
- **Features**:
  - New password input with strength validation
  - Confirm password with match validation
  - Token and email from URL query parameters
  - Password requirements display
  - Form validation (min 6 chars, uppercase, lowercase, number)
  - Success message with auto-redirect to login
  - Error handling for expired tokens
  - Back to login navigation
  - Loading states during submission
- **Styling**: Indigo gradient theme
- **API Endpoint**: `POST /api/account/reset-password`
- **Request**: `{ email: string, token: string, newPassword: string }`
- **Response**: `{ message: string }`

## User Roles

The authentication system supports three distinct user roles:

- **Admin**: Platform administrators with full system access
- **Merchant**: Store owners who manage products, orders, and their storefronts
- **Customer**: Shoppers who browse and purchase products

## Authentication Flows

### Login Flow

1. User selects their role-specific login page
2. Enters email and password credentials
3. Form validates input (required fields, email format, password length)
4. On submit, `AuthService` sends credentials to IdentityServer backend
5. On success:
   - JWT tokens stored in localStorage
   - User state updated via signals
   - Redirects to role-specific dashboard
6. On failure:
   - Error message displayed
   - Form remains editable

### Forgot Password Flow

1. User clicks "Forgot Password?" link on any login page
2. Redirected to `/forgot-password` page
3. Enters email address
4. Form validates email format
5. On submit, `AuthService.forgotPassword()` sends email to backend
6. Backend sends password reset email with token
7. On success:
   - Success message displayed
   - User instructed to check email
8. On failure:
   - Error message displayed (network error, invalid email, etc.)

### Reset Password Flow

1. User receives password reset email with link
2. Link contains token and email as query parameters:
   - Example: `/reset-password?token=abc123&email=user@example.com`
3. User clicks link and is redirected to `/reset-password` page
4. Component extracts token and email from URL
5. User enters new password and confirms it
6. Form validates:
   - Password strength (min 6 chars, uppercase, lowercase, number)
   - Password confirmation matches
7. On submit, `AuthService.resetPassword()` sends request to backend
8. On success:
   - Success message displayed
   - Auto-redirect to login page after 2 seconds
9. On failure:
   - Error message displayed (expired token, invalid token, etc.)
   - User can request new reset link

## State Management

The module uses **signal-based state management** as per technical decisions:
- `currentUser` signal for user information
- `isAuthenticated` signal for auth status
- `isLoading` signal for async operations
- `errorMessage` signal for error states

## Integration with IdentityServer

The authentication service now connects to the backend IdentityServer API:

### API Endpoints

| Endpoint | Method | Description | Request Body | Response |
|----------|--------|-------------|--------------|----------|
| `/api/account/login` | POST | User login | `{ email, password, role }` | `{ accessToken, refreshToken, user, expiresIn }` |
| `/api/account/forgot-password` | POST | Request password reset | `{ email }` | `{ message }` |
| `/api/account/reset-password` | POST | Reset password with token | `{ email, token, newPassword }` | `{ message }` |
| `/api/account/refresh-token` | POST | Refresh access token | `{ refreshToken }` | `{ accessToken, refreshToken, user, expiresIn }` |

### Configuration

Environment-specific API URLs are configured in:
- Development: `/environments/environment.ts` (default: `http://localhost:5001/api`)
- Production: `/environments/environment.prod.ts` (default: `https://api.vendo.com/api`)

### Token Management

- Access tokens stored in localStorage as `access_token`
- Refresh tokens stored in localStorage as `refresh_token`
- User data stored in localStorage as `user` (JSON string)
- Auth interceptor automatically adds Bearer token to all HTTP requests
- Token refresh logic implemented (TODO: Auto-refresh on 401 responses)

## Security Considerations

- Passwords are never stored locally
- JWT tokens stored in localStorage (consider httpOnly cookies for production)
- Auth interceptor adds bearer token to all HTTP requests
- 401 responses trigger automatic logout and redirect
- Form inputs use proper input types (email, password)

## Routing

All auth routes are lazy-loaded through the `AuthRoutingModule`:
- `/login` → Redirects to `/login/customer`
- `/login/admin` → Admin login
- `/login/merchant` → Merchant login
- `/login/customer` → Customer login
- `/forgot-password` → Forgot password form
- `/reset-password` → Reset password form (requires token and email query params)

## Styling

- Uses **Tailwind CSS only** (no UI component libraries)
- Each login type has a unique color scheme for easy visual distinction
- Responsive design for mobile and desktop
- Accessibility features included (labels, ARIA attributes)
- Loading spinners for better UX during async operations

## Testing the Authentication Flow

### Local Development Setup

1. **Start the backend IdentityServer** (default: `http://localhost:5001`)
2. **Start the Angular development server**: `ng serve`
3. **Navigate to login pages**:
   - Admin: `http://localhost:4200/login/admin`
   - Merchant: `http://localhost:4200/login/merchant`
   - Customer: `http://localhost:4200/login/customer`

### Testing Login

1. Enter valid credentials
2. Verify JWT tokens are stored in localStorage
3. Verify redirect to appropriate dashboard
4. Test with invalid credentials to see error handling

### Testing Forgot Password

1. Navigate to any login page
2. Click "Forgot Password?" link
3. Enter email address
4. Verify success message appears
5. Check backend logs for password reset email
6. Test with invalid email to see error handling

### Testing Reset Password

1. Get reset token from backend (email or logs)
2. Navigate to: `/reset-password?token=YOUR_TOKEN&email=user@example.com`
3. Enter new password (must meet requirements)
4. Confirm password
5. Verify success message and auto-redirect
6. Test login with new password
7. Test with expired token to see error handling

### Testing Error Scenarios

1. **Network errors**: Stop backend server and attempt login
2. **Invalid credentials**: Enter wrong email/password
3. **Expired tokens**: Use old reset token
4. **Invalid email format**: Enter malformed email
5. **Weak passwords**: Test password validation

## Future Enhancements

- [x] Implement "Forgot Password" functionality
- [x] Implement "Reset Password" functionality
- [ ] Add automatic token refresh on 401 responses
- [ ] Add two-factor authentication (2FA)
- [ ] Implement social login (OAuth providers)
- [ ] Add CAPTCHA for bot prevention
- [ ] Implement account lockout after failed attempts
- [ ] Add session timeout warnings
- [ ] Implement "Remember me" token refresh
- [ ] Move tokens from localStorage to httpOnly cookies for better security
