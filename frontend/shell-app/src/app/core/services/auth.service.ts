import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, catchError, map, of, tap } from 'rxjs';
import { User, UserRole } from '../models/user.model';
import { environment } from '../../../environments/environment';

/**
 * BFF-based AuthService
 *
 * This service works with the Backend for Frontend (BFF) pattern.
 * All OAuth/OIDC complexity is handled by the BFF service.
 * The Angular app only communicates with the BFF via HTTP calls.
 *
 * Key features:
 * - Tokens stored in HTTP-only cookies (managed by BFF)
 * - No tokens exposed to JavaScript
 * - Authorization Code + PKCE flow handled by BFF
 * - Automatic token refresh by BFF
 * - Anti-CSRF protection
 * - Role-based access control
 * - Multi-tenant support
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Signal-based state management
  private currentUserSignal = signal<User | null>(null);
  private isAuthenticatedSignal = signal<boolean>(false);
  private isAuthenticationReadySubject = new BehaviorSubject<boolean>(false);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = this.isAuthenticatedSignal.asReadonly();
  readonly isAuthenticationReady$ = this.isAuthenticationReadySubject.asObservable();

  private readonly bffUrl = environment.bffUrl;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Check authentication status on service initialization
    this.checkAuthenticationStatus();
  }

  /**
   * Check if user is already authenticated by calling BFF user endpoint
   */
  private checkAuthenticationStatus(): void {
    this.getUserFromBff().subscribe({
      next: (user) => {
        if (user) {
          this.currentUserSignal.set(user);
          this.isAuthenticatedSignal.set(true);
        }
        this.isAuthenticationReadySubject.next(true);
      },
      error: () => {
        this.isAuthenticationReadySubject.next(true);
      }
    });
  }

  /**
   * Get user information from BFF
   * BFF validates the HTTP-only cookie and returns user claims
   */
  private getUserFromBff(): Observable<User | null> {
    return this.http.get<any>(`${this.bffUrl}/bff/user`, {
      withCredentials: true // Important: send cookies
    }).pipe(
      map(claims => {
        if (!claims || claims.length === 0) {
          return null;
        }

        // BFF returns claims as array of {type, value} objects
        const claimsMap = this.convertClaimsArrayToMap(claims);

        return {
          id: claimsMap['sub'] || '',
          email: claimsMap['email'] || '',
          role: this.mapClaimToRole(claimsMap['role']),
          firstName: claimsMap['given_name'] || claimsMap['name']?.split(' ')[0] || '',
          lastName: claimsMap['family_name'] || claimsMap['name']?.split(' ').slice(1).join(' ') || '',
          tenantId: claimsMap['tenant_id']
        };
      }),
      catchError(() => of(null))
    );
  }

  /**
   * Convert BFF claims array to map for easier access
   * BFF returns claims as: [{type: "sub", value: "123"}, ...]
   */
  private convertClaimsArrayToMap(claims: any[]): Record<string, any> {
    const map: Record<string, any> = {};
    claims.forEach(claim => {
      map[claim.type] = claim.value;
    });
    return map;
  }

  /**
   * Initiate login by redirecting to BFF login endpoint
   * BFF will redirect to Identity Service for authentication
   */
  login(): void {
    // Redirect to BFF login endpoint
    // BFF will initiate OAuth Authorization Code + PKCE flow
    window.location.href = `${this.bffUrl}/bff/login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
  }

  /**
   * Logout current user
   * This calls BFF logout which performs both local and remote logout
   */
  logout(): void {
    this.http.get(`${this.bffUrl}/bff/logout`, {
      withCredentials: true
    }).subscribe({
      next: () => {
        this.clearUserData();
        this.router.navigate(['/login']);
      },
      error: () => {
        // Even if logout fails on server, clear local state
        this.clearUserData();
        this.router.navigate(['/login']);
      }
    });
  }

  /**
   * Clear user data from state
   */
  private clearUserData(): void {
    this.currentUserSignal.set(null);
    this.isAuthenticatedSignal.set(false);
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
   * Refresh user information from BFF
   */
  refreshUser(): Observable<User | null> {
    return this.getUserFromBff().pipe(
      tap(user => {
        if (user) {
          this.currentUserSignal.set(user);
          this.isAuthenticatedSignal.set(true);
        } else {
          this.clearUserData();
        }
      })
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

  /**
   * Request password reset
   * Sends reset link to user's email
   */
  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.bffUrl}/api/identity/account/forgot-password`,
      { email },
      { withCredentials: true }
    );
  }

  /**
   * Reset password with token
   * Token and email are sent from the reset link
   */
  resetPassword(email: string, token: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.bffUrl}/api/identity/account/reset-password`,
      { email, token, newPassword },
      { withCredentials: true }
    );
  }
}
