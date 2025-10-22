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
  - Link to customer registration
  - Links to other login types
- **Styling**: Blue gradient theme

## User Roles

The authentication system supports three distinct user roles:

- **Admin**: Platform administrators with full system access
- **Merchant**: Store owners who manage products, orders, and their storefronts
- **Customer**: Shoppers who browse and purchase products

## Authentication Flow

1. User selects their role-specific login page
2. Enters email and password credentials
3. Form validates input (required fields, email format, password length)
4. On submit, `AuthService` sends credentials to IdentityServer (currently mocked)
5. On success:
   - JWT tokens stored in localStorage
   - User state updated via signals
   - Redirects to role-specific dashboard
6. On failure:
   - Error message displayed
   - Form remains editable

## State Management

The module uses **signal-based state management** as per technical decisions:
- `currentUser` signal for user information
- `isAuthenticated` signal for auth status
- `isLoading` signal for async operations
- `errorMessage` signal for error states

## Integration with IdentityServer

Currently, the login functionality uses mock data for demonstration purposes. In production:

1. Replace mock responses in `auth.service.ts` with actual HTTP calls to IdentityServer
2. Implement proper token refresh logic
3. Add token expiration handling
4. Implement logout functionality with server-side session invalidation

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

## Styling

- Uses **Tailwind CSS only** (no UI component libraries)
- Each login type has a unique color scheme for easy visual distinction
- Responsive design for mobile and desktop
- Accessibility features included (labels, ARIA attributes)
- Loading spinners for better UX during async operations

## Future Enhancements

- [ ] Implement "Forgot Password" functionality
- [ ] Add two-factor authentication (2FA)
- [ ] Implement social login (OAuth providers)
- [ ] Add CAPTCHA for bot prevention
- [ ] Implement account lockout after failed attempts
- [ ] Add session timeout warnings
- [ ] Implement "Remember me" token refresh
