import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

/**
 * Auth Callback Component
 *
 * This component handles the OAuth2/OIDC callback after the user
 * successfully authenticates with IdentityServer.
 *
 * The authorization code is exchanged for tokens, and the user
 * is redirected to the appropriate dashboard based on their role.
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
    this.handleCallback();
  }

  private handleCallback(): void {
    this.authService.handleCallback().subscribe({
      next: (success) => {
        if (success) {
          this.redirectToRoleDashboard();
        } else {
          this.errorMessage.set('Authentication failed. Please try again.');
          this.isProcessing.set(false);
        }
      },
      error: (error) => {
        console.error('OAuth callback error:', error);
        this.errorMessage.set('An error occurred during authentication.');
        this.isProcessing.set(false);
      }
    });
  }

  private redirectToRoleDashboard(): void {
    const role = this.authService.getUserRole();

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
    this.router.navigate(['/login/customer']);
  }
}
