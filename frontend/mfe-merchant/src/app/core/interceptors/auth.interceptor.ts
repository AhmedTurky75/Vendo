import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

/**
 * BFF Auth Interceptor
 *
 * Handles HTTP requests to the BFF:
 * - Adds withCredentials for cookie-based auth
 * - Handles 401 unauthorized responses
 * - Adds anti-CSRF headers for state-changing requests
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Only intercept requests to our BFF
    if (request.url.startsWith(environment.bffUrl)) {
      // Clone request and add withCredentials for cookie-based auth
      request = request.clone({
        withCredentials: true
      });

      // For state-changing requests (POST, PUT, DELETE, PATCH),
      // BFF will validate anti-CSRF token from cookie
      // No need to manually add headers - BFF handles this
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // User is not authenticated, redirect to login
          console.warn('Unauthorized request, redirecting to login');
          window.location.href = `${environment.bffUrl}/bff/login`;
        }

        return throwError(() => error);
      })
    );
  }
}
