import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/user.model';

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
  ): boolean {
    if (!this.authService.isAuthenticated()) {
      // Not logged in, redirect to appropriate login page
      this.router.navigate(['/login/customer']);
      return false;
    }

    // Check if route requires specific role
    const requiredRole = route.data['role'] as UserRole;
    if (requiredRole) {
      const hasRole = this.authService.hasRole(requiredRole);
      if (!hasRole) {
        // User doesn't have required role, redirect to unauthorized page
        this.router.navigate(['/unauthorized']);
        return false;
      }
    }

    return true;
  }
}
