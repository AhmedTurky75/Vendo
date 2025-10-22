import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

@Component({
  selector: 'app-merchant-login',
    standalone: false,

  templateUrl: './merchant-login.component.html',
  styleUrls: ['./merchant-login.component.css']
})
export class MerchantLoginComponent {
  loginForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword.set(!this.showPassword());
  }

  onSubmit(): void {
    // Note: With OAuth2/OIDC, the form fields are not used for direct authentication
    // The user will be redirected to IdentityServer for authentication
    // This form is kept for UI consistency, but actual auth happens on IdentityServer

    this.isLoading.set(true);
    this.errorMessage.set(null);

    // Redirect to IdentityServer for authentication with PKCE flow
    this.authService.login(UserRole.Merchant);

    // Note: The page will redirect, so loading state may not be visible
    // The user will be brought back to /auth/callback after authentication
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }
}
