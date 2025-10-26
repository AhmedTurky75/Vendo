import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, map, take } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Auth Guard for protecting routes
 *
 * Checks if user is authenticated via BFF before allowing access
 * Redirects to login if not authenticated
 */
@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> {
    return this.authService.isAuthenticationReady$.pipe(
      take(1),
      map(() => {
        if (this.authService.isAuthenticated()) {
          return true;
        }

        // Not authenticated, redirect to login
        this.authService.login();
        return false;
      })
    );
  }
}
