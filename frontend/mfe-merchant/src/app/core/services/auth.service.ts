import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  email: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  tenantId?: string;
}

/**
 * BFF-based AuthService for Merchant Portal
 *
 * This service works with the Backend for Frontend (BFF) pattern.
 * All OAuth/OIDC complexity is handled by the Merchant BFF service.
 * The Angular app only communicates with the BFF via HTTP calls.
 *
 * Key features:
 * - Tokens stored in HTTP-only cookies (managed by BFF)
 * - No tokens exposed to JavaScript
 * - Authorization Code + PKCE flow handled by BFF
 * - Automatic token refresh by BFF
 * - Anti-CSRF protection
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
          role: claimsMap['role'],
          firstName: claimsMap['given_name'] || claimsMap['name']?.split(' ')[0] || '',
          lastName: claimsMap['family_name'] || claimsMap['name']?.split(' ').slice(1).join(' ') || '',
          name: claimsMap['name'],
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
      if (claim.type && claim.value !== undefined) {
        map[claim.type] = claim.value;
      }
    });
    return map;
  }

  /**
   * Initiate login by redirecting to BFF login endpoint
   * BFF will handle OAuth flow with IdentityServer
   */
  login(): void {
    // Store return URL before redirecting
    const returnUrl = this.router.url;
    if (returnUrl !== '/') {
      sessionStorage.setItem('merchant_return_url', returnUrl);
    }

    // Redirect to BFF login endpoint
    window.location.href = `${this.bffUrl}/bff/login`;
  }

  /**
   * Handle OAuth callback after successful authentication
   */
  handleCallback(): Observable<User | null> {
    return this.getUserFromBff().pipe(
      map(user => {
        if (user) {
          this.currentUserSignal.set(user);
          this.isAuthenticatedSignal.set(true);

          // Get return URL and navigate
          const returnUrl = sessionStorage.getItem('merchant_return_url') || '/merchant/dashboard';
          sessionStorage.removeItem('merchant_return_url');
          this.router.navigateByUrl(returnUrl);
        }
        return user;
      })
    );
  }

  /**
   * Logout user by calling BFF logout endpoint
   * BFF will clear the HTTP-only cookie and redirect to IdentityServer
   */
  logout(): void {
    this.currentUserSignal.set(null);
    this.isAuthenticatedSignal.set(false);

    // Redirect to BFF logout endpoint
    window.location.href = `${this.bffUrl}/bff/logout`;
  }

  /**
   * Get user info (triggers BFF to fetch fresh user info)
   */
  getUserInfo(): Observable<User | null> {
    return this.getUserFromBff().pipe(
      map(user => {
        if (user) {
          this.currentUserSignal.set(user);
          this.isAuthenticatedSignal.set(true);
        } else {
          this.currentUserSignal.set(null);
          this.isAuthenticatedSignal.set(false);
        }
        return user;
      })
    );
  }

  /**
   * Check if user has a specific role
   */
  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.role?.toLowerCase() === role.toLowerCase();
  }

  /**
   * Check if user is a merchant
   */
  isMerchant(): boolean {
    return this.hasRole('Merchant') || this.hasRole('Admin');
  }
}
