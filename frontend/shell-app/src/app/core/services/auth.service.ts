import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, from, BehaviorSubject } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { OAuthService, OAuthEvent, OAuthErrorEvent } from 'angular-oauth2-oidc';
import { User, UserRole } from '../models/user.model';
import { environment } from '../../../environments/environment';
import { authConfig, getAuthConfigForRole } from '../config/oauth.config';

/**
 * Enhanced AuthService with OAuth2/OIDC support
 *
 * This service uses angular-oauth2-oidc library to implement
 * Authorization Code Flow with PKCE (Proof Key for Code Exchange).
 *
 * Key features:
 * - Authorization Code + PKCE flow (most secure for SPAs)
 * - Automatic token refresh using refresh tokens
 * - Silent refresh via hidden iframe
 * - Secure token storage in sessionStorage
 * - Role-based access control
 * - Multi-tenant support
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Signal-based state management as per tech decisions
  private currentUserSignal = signal<User | null>(null);
  private isAuthenticatedSignal = signal<boolean>(false);
  private isAuthenticationReadySubject = new BehaviorSubject<boolean>(false);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = this.isAuthenticatedSignal.asReadonly();
  readonly isAuthenticationReady$ = this.isAuthenticationReadySubject.asObservable();

  private readonly apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private oauthService: OAuthService,
    private router: Router
  ) {
    this.configureOAuth();
    this.setupOAuthEventHandlers();
  }

  /**
   * Configure OAuth2/OIDC settings
   */
  private configureOAuth(): void {
    // Use sessionStorage instead of localStorage for better security
    // SessionStorage is cleared when browser tab is closed
    this.oauthService.configure(authConfig);

    // Use sessionStorage for tokens (more secure than localStorage)
    this.oauthService.setStorage(sessionStorage);

    // Load discovery document and try to login automatically
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      if (this.oauthService.hasValidAccessToken()) {
        this.loadUserProfile();
      } else {
        this.isAuthenticationReadySubject.next(true);
      }

      // Setup automatic silent refresh
      this.oauthService.setupAutomaticSilentRefresh();
    }).catch(err => {
      console.error('Error loading discovery document', err);
      this.isAuthenticationReadySubject.next(true);
    });
  }

  /**
   * Setup OAuth event handlers
   */
  private setupOAuthEventHandlers(): void {
    // Listen to OAuth events
    this.oauthService.events
      .subscribe((event: OAuthEvent) => {
        if (event.type === 'token_received') {
          this.loadUserProfile();
        }

        if (event.type === 'token_expires') {
          console.log('Token is about to expire');
        }

        if (event.type === 'logout') {
          this.clearUserData();
        }

        if (event instanceof OAuthErrorEvent) {
          console.error('OAuth error', event);
        }
      });
  }

  /**
   * Initialize authentication by redirecting to IdentityServer
   * @param role User role to customize login experience
   */
  login(role?: UserRole): void {
    // Configure with role-specific settings if provided
    if (role) {
      const config = getAuthConfigForRole(role);
      this.oauthService.configure(config);
    }

    // Initiate Authorization Code Flow with PKCE
    this.oauthService.initCodeFlow();
  }

  /**
   * Handle OAuth callback after redirect from IdentityServer
   */
  handleCallback(): Observable<boolean> {
    return from(this.oauthService.loadDiscoveryDocumentAndTryLogin()).pipe(
      map(() => {
        if (this.oauthService.hasValidAccessToken()) {
          this.loadUserProfile();
          return true;
        }
        return false;
      })
    );
  }

  /**
   * Logout current user
   * This will revoke tokens and redirect to IdentityServer logout
   */
  logout(): void {
    // Revoke refresh token if available (for security)
    this.oauthService.revokeTokenAndLogout();

    // Clear local user data
    this.clearUserData();
  }

  /**
   * Logout without revoking token (local logout only)
   */
  logoutLocally(): void {
    this.oauthService.logOut(false);
    this.clearUserData();
  }

  /**
   * Clear user data from state
   */
  private clearUserData(): void {
    this.currentUserSignal.set(null);
    this.isAuthenticatedSignal.set(false);
  }

  /**
   * Load user profile from ID token claims
   */
  private loadUserProfile(): void {
    const claims = this.oauthService.getIdentityClaims();

    if (claims) {
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
      this.isAuthenticationReadySubject.next(true);
    }
  }

  /**
   * Map claim role to UserRole enum
   */
  private mapClaimToRole(roleClaim: string | string[]): UserRole {
    const roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];

    // Priority: Admin > Merchant > Customer
    if (roles.includes('Admin')) return UserRole.Admin;
    if (roles.includes('Merchant')) return UserRole.Merchant;
    return UserRole.Customer;
  }

  /**
   * Get user info from IdentityServer userinfo endpoint
   */
  getUserInfo(): Observable<any> {
    return from(this.oauthService.loadUserProfile());
  }

  /**
   * Request password reset
   * Sends reset link to user's email
   */
  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/account/forgot-password`,
      { email }
    );
  }

  /**
   * Reset password with token
   * Token and email are sent from the reset link
   */
  resetPassword(email: string, token: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/account/reset-password`,
      { email, token, newPassword }
    );
  }

  /**
   * Refresh access token using refresh token
   * This is handled automatically by the library, but can be called manually
   */
  refreshToken(): Observable<boolean> {
    return from(this.oauthService.refreshToken()).pipe(
      map(() => this.oauthService.hasValidAccessToken())
    );
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: UserRole): boolean {
    const user = this.currentUserSignal();
    return user?.role === role;
  }

  /**
   * Check if user has any of the specified roles
   */
  hasAnyRole(roles: UserRole[]): boolean {
    const user = this.currentUserSignal();
    return user ? roles.includes(user.role) : false;
  }

  /**
   * Get current user's role
   */
  getUserRole(): UserRole | null {
    const user = this.currentUserSignal();
    return user?.role || null;
  }

  /**
   * Get access token from OAuthService
   */
  getAccessToken(): string | null {
    return this.oauthService.getAccessToken();
  }

  /**
   * Get ID token from OAuthService
   */
  getIdToken(): string | null {
    return this.oauthService.getIdToken();
  }

  /**
   * Check if access token is valid
   */
  hasValidAccessToken(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  /**
   * Check if ID token is valid
   */
  hasValidIdToken(): boolean {
    return this.oauthService.hasValidIdToken();
  }

  /**
   * Get token expiration time
   */
  getAccessTokenExpiration(): number {
    return this.oauthService.getAccessTokenExpiration();
  }

  /**
   * Get identity claims from ID token
   */
  getIdentityClaims(): Record<string, any> {
    return this.oauthService.getIdentityClaims() as Record<string, any>;
  }

  /**
   * Get user's tenant ID
   */
  getTenantId(): string | null {
    const user = this.currentUserSignal();
    return user?.tenantId || null;
  }

  /**
   * Check if user is authenticated (for compatibility)
   */
  isUserAuthenticated(): boolean {
    return this.isAuthenticatedSignal();
  }
}
