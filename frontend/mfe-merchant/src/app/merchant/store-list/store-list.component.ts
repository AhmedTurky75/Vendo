import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';
import { Store } from '../../core/models/store.model';

@Component({
  selector: 'app-store-list',
  standalone: false,
  templateUrl: './store-list.component.html',
  styleUrls: ['./store-list.component.css']
})
export class StoreListComponent implements OnInit {
  stores = signal<Store[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor(
    private storeService: StoreService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadStores();
  }

  /**
   * Load all stores for the current merchant
   */
  loadStores(): void {
    const currentUser = this.authService.currentUser();

    if (!currentUser) {
      this.errorMessage.set('User not authenticated. Please log in again.');
      this.router.navigate(['/login/merchant']);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.storeService.getStoresByOwner(currentUser.id).subscribe({
      next: (stores :any) => {
        this.isLoading.set(false);
        this.stores.set(stores);

        // If no stores, redirect to onboarding
        if (stores.length === 0) {
          this.router.navigate(['/merchant/onboarding']);
        }
      },
      error: (error:any) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Failed to load stores. Please try again.');
        console.error('Error loading stores:', error);
      }
    });
  }

  /**
   * Navigate to store settings
   */
  openStoreSettings(storeId: string): void {
    this.router.navigate(['/merchant/stores', storeId, 'settings']);
  }

  /**
   * Navigate to create new store
   */
  createNewStore(): void {
    this.router.navigate(['/merchant/onboarding']);
  }

  /**
   * Open store in new tab (placeholder - actual URL will be implemented later)
   */
  visitStore(subdomain: string): void {
    const storeUrl = `${window.location.origin}/stores/${subdomain}`;
    window.open(storeUrl, '_blank');
  }

  /**
   * Logout current user
   */
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login/merchant']);
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
