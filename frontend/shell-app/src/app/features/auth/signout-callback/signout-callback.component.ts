import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Signout Callback Component
 *
 * This component handles the callback after the user
 * successfully signs out from IdentityServer.
 *
 * After a brief delay to show a confirmation message,
 * the user is redirected to the login page.
 */
@Component({
  selector: 'app-signout-callback',
  standalone: false,
  templateUrl: './signout-callback.component.html',
  styleUrls: ['./signout-callback.component.css']
})
export class SignoutCallbackComponent implements OnInit {
  constructor(private router: Router) {}

  ngOnInit(): void {
    // Wait 2 seconds before redirecting to login
    setTimeout(() => {
      this.router.navigate(['/login/customer']);
    }, 2000);
  }

  navigateToLogin(): void {
    this.router.navigate(['/login/customer']);
  }
}
