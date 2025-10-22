import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

/**
 * BFF-based AuthInterceptor
 *
 * This interceptor:
 * 1. Ensures all requests to BFF include credentials (cookies)
 * 2. Adds anti-CSRF header for state-changing requests
 * 3. Handles 401 Unauthorized by redirecting to login
 * 4. Does NOT add Bearer tokens (BFF manages tokens via cookies)
 *
 * The BFF automatically handles token refresh, so no manual refresh logic is needed.
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private readonly bffUrl = environment.bffUrl;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Clone request and add credentials for BFF requests
    if (this.isBffRequest(request.url)) {
      request = request.clone({
        withCredentials: true // Send HTTP-only cookies
      });

      // Add anti-CSRF header for state-changing requests
      // BFF validates this header to prevent CSRF attacks
      if (this.isStateChangingRequest(request.method)) {
        request = request.clone({
          setHeaders: {
            'X-CSRF': '1' // Simple anti-CSRF token
          }
        });
      }
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Unauthorized - session expired or user not logged in
          console.log('401 Unauthorized, redirecting to login');
          this.redirectToLogin();
        }

        if (error.status === 403) {
          // Forbidden - user doesn't have permission
          console.error('403 Forbidden - Access denied:', error);
          this.router.navigate(['/unauthorized']);
        }

        return throwError(() => error);
      })
    );
  }

  /**
   * Check if request is going to BFF
   */
  private isBffRequest(url: string): boolean {
    return url.startsWith(this.bffUrl) || url.startsWith('/bff') || url.startsWith('/api');
  }

  /**
   * Check if request method is state-changing (requires CSRF protection)
   */
  private isStateChangingRequest(method: string): boolean {
    return ['POST', 'PUT', 'DELETE', 'PATCH'].includes(method.toUpperCase());
  }

  /**
   * Redirect to login
   */
  private redirectToLogin(): void {
    // Store current URL for redirect after login
    const currentUrl = this.router.url;
    if (currentUrl && currentUrl !== '/login') {
      sessionStorage.setItem('redirect_url', currentUrl);
    }

    this.router.navigate(['/login']);
  }
}
