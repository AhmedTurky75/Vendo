import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Enhanced AuthInterceptor with OAuth2/OIDC support
 *
 * This interceptor:
 * 1. Adds the OIDC access token to outgoing API requests
 * 2. Handles token expiration and automatic refresh
 * 3. Handles 401 Unauthorized responses
 * 4. Implements request queuing during token refresh
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Don't add token to IdentityServer token endpoint requests
    if (this.isTokenEndpoint(request.url)) {
      return next.handle(request);
    }

    // Add access token to request if available
    const token = this.authService.getAccessToken();
    if (token) {
      request = this.addToken(request, token);
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expired or invalid, try to refresh
          return this.handle401Error(request, next);
        }

        if (error.status === 403) {
          // Forbidden - user doesn't have permission
          console.error('Access forbidden:', error);
        }

        return throwError(() => error);
      })
    );
  }

  /**
   * Add access token to request headers
   */
  private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  /**
   * Handle 401 Unauthorized error by attempting to refresh token
   */
  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      // Check if we have a valid token before trying to refresh
      if (this.authService.hasValidAccessToken()) {
        // Token is still valid according to the library, proceed with request
        this.isRefreshing = false;
        return next.handle(request);
      }

      // Attempt to refresh the token
      return this.authService.refreshToken().pipe(
        switchMap((success: boolean) => {
          this.isRefreshing = false;

          if (success) {
            const newToken = this.authService.getAccessToken();
            this.refreshTokenSubject.next(newToken);
            return next.handle(this.addToken(request, newToken!));
          } else {
            // Refresh failed, redirect to login
            this.redirectToLogin();
            return throwError(() => new Error('Token refresh failed'));
          }
        }),
        catchError((err) => {
          this.isRefreshing = false;
          this.redirectToLogin();
          return throwError(() => err);
        })
      );
    } else {
      // Token refresh is already in progress, queue this request
      return this.refreshTokenSubject.pipe(
        filter(token => token != null),
        take(1),
        switchMap(token => {
          return next.handle(this.addToken(request, token));
        })
      );
    }
  }

  /**
   * Redirect to login and clear authentication
   */
  private redirectToLogin(): void {
    this.authService.logoutLocally();
    this.router.navigate(['/login/customer']);
  }

  /**
   * Check if URL is a token endpoint (should not add auth header)
   */
  private isTokenEndpoint(url: string): boolean {
    return url.includes('/connect/token') ||
           url.includes('/connect/authorize') ||
           url.includes('/connect/revocation') ||
           url.includes('/.well-known/');
  }
}
