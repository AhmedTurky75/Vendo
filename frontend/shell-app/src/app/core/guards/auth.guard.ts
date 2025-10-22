import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/user.model';

/**
 * BFF-based AuthGuard
 *
 * This guard protects routes from unauthorized access by checking:
 * 1. If the user is authenticated (has valid session cookie)
 * 2. If the user has the required role for the route
 *
 * If authentication fails, the user is redirected to login page.
 * The BFF handles all OAuth/OIDC complexity.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    // Wait for authentication to be ready
    return this.authService.isAuthenticationReady$.pipe(
      take(1),
      map(() => {
        // Check if user is authenticated
        if (!this.authService.isUserAuthenticated()) {
          console.log('User not authenticated, redirecting to login');
          // Store the attempted URL for redirecting after login
          sessionStorage.setItem('redirect_url', state.url);
          return this.router.createUrlTree(['/login']);
        }

        // Check if route requires specific role
        const requiredRole = route.data['role'] as UserRole;
        if (requiredRole) {
          const hasRole = this.authService.hasRole(requiredRole);
          if (!hasRole) {
            console.log(`User does not have required role: ${requiredRole}`);
            // User doesn't have required role, redirect to unauthorized page
            return this.router.createUrlTree(['/unauthorized']);
          }
        }

        // Check if route requires any of multiple roles
        const requiredRoles = route.data['roles'] as UserRole[];
        if (requiredRoles && requiredRoles.length > 0) {
          const hasAnyRole = this.authService.hasAnyRole(requiredRoles);
          if (!hasAnyRole) {
            console.log(`User does not have any of required roles: ${requiredRoles.join(', ')}`);
            return this.router.createUrlTree(['/unauthorized']);
          }
        }

        return true;
      })
    );
  }
}
