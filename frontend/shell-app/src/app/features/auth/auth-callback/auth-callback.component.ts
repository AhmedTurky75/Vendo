import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

/**
 * Auth Callback Component
 *
 * With BFF pattern, the OAuth callback is handled by the BFF service.
 * This component simply checks if the user is authenticated and redirects
 * them to the appropriate dashboard based on their role.
 *
 * BFF handles the OAuth Authorization Code exchange at /signin-oidc
 * and then redirects back to the Angular app.
 */
@Component({
  selector: 'app-auth-callback',
  standalone: false,
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.css']
})
export class AuthCallbackComponent implements OnInit {
  isProcessing = signal(true);
  errorMessage = signal<string | null>(null);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.handlePostLogin();
  }

  private handlePostLogin(): void {
    // Wait for auth service to be ready
    this.authService.isAuthenticationReady$.subscribe(ready => {
      if (ready) {
        if (this.authService.isUserAuthenticated()) {
          // User is authenticated, redirect to role-based dashboard
          this.redirectToRoleDashboard();
        } else {
          // Not authenticated, redirect to login
          this.errorMessage.set('Authentication failed. Please try again.');
          this.isProcessing.set(false);
          setTimeout(() => {
            this.router.navigate(['/login/admin']);
          }, 2000);
        }
      }
    });
  }

  private redirectToRoleDashboard(): void {
    const role = this.authService.getUserRole();

    // Get the stored redirect URL if it exists
    const redirectUrl = sessionStorage.getItem('redirect_url');
    if (redirectUrl) {
      sessionStorage.removeItem('redirect_url');
      this.router.navigateByUrl(redirectUrl);
      return;
    }

    // Default redirects based on role
    switch (role) {
      case UserRole.Admin:
        this.router.navigate(['/admin/dashboard']);
        break;
      case UserRole.Merchant:
        this.router.navigate(['/merchant/dashboard']);
        break;
      case UserRole.Customer:
        this.router.navigate(['/shop']);
        break;
      default:
        this.router.navigate(['/']);
    }
  }

  retryLogin(): void {
    this.router.navigate(['/login/admin']);
  }
}
