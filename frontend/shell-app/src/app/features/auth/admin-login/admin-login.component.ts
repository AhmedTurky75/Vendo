import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
    standalone: false,
  templateUrl: './admin-login.component.html',
  styleUrls: ['./admin-login.component.css']
})
export class AdminLoginComponent {
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
    // Note: With BFF pattern, the form fields are not used for direct authentication
    // The user will be redirected to BFF -> IdentityServer for authentication
    // This form is kept for UI consistency, but actual auth happens via BFF OAuth2/OIDC + PKCE

    this.isLoading.set(true);
    this.errorMessage.set(null);

    // Redirect to BFF which will initiate OAuth Authorization Code + PKCE flow
    this.authService.login();

    // Note: The page will redirect to BFF, then IdentityServer
    // After authentication, user will be redirected back to the app
  }

  get email() {
    return this.loginForm.get('email');
  }

  get password() {
    return this.loginForm.get('password');
  }
}
