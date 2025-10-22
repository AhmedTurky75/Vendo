import { Injectable, signal } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { delay, tap } from 'rxjs/operators';
import { User, UserRole, LoginRequest, LoginResponse } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Signal-based state management as per tech decisions
  private currentUserSignal = signal<User | null>(null);
  private isAuthenticatedSignal = signal<boolean>(false);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = this.isAuthenticatedSignal.asReadonly();

  constructor() {
    this.loadUserFromStorage();
  }

  /**
   * Login method for all user roles
   * In production, this would call the IdentityServer API
   */
  login(loginRequest: LoginRequest): Observable<LoginResponse> {
    // TODO: Replace with actual HTTP call to IdentityServer
    // For now, returning mock data for demonstration

    // Simulate API call
    return of({
      accessToken: 'mock-access-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      user: {
        id: '123',
        email: loginRequest.email,
        role: loginRequest.role,
        firstName: 'Test',
        lastName: 'User',
        tenantId: loginRequest.role === UserRole.Customer ? undefined : 'tenant-123'
      },
      expiresIn: 3600
    }).pipe(
      delay(1000), // Simulate network delay
      tap(response => {
        this.setSession(response);
      })
    );
  }

  /**
   * Logout current user
   */
  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user');
    this.currentUserSignal.set(null);
    this.isAuthenticatedSignal.set(false);
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: UserRole): boolean {
    const user = this.currentUserSignal();
    return user?.role === role;
  }

  /**
   * Get current user's role
   */
  getUserRole(): UserRole | null {
    const user = this.currentUserSignal();
    return user?.role || null;
  }

  /**
   * Store authentication session
   */
  private setSession(authResult: LoginResponse): void {
    localStorage.setItem('access_token', authResult.accessToken);
    localStorage.setItem('refresh_token', authResult.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResult.user));

    this.currentUserSignal.set(authResult.user);
    this.isAuthenticatedSignal.set(true);
  }

  /**
   * Load user data from localStorage on app initialization
   */
  private loadUserFromStorage(): void {
    const userJson = localStorage.getItem('user');
    const token = localStorage.getItem('access_token');

    if (userJson && token) {
      try {
        const user = JSON.parse(userJson) as User;
        this.currentUserSignal.set(user);
        this.isAuthenticatedSignal.set(true);
      } catch (error) {
        // Invalid stored data, clear it
        this.logout();
      }
    }
  }
}
