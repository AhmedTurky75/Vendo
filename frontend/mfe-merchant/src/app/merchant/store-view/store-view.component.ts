import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { StoreService } from '../../core/services/store.service';
import { Store } from '../../core/models/store.model';

@Component({
  selector: 'app-store-view',
  standalone: false,
  templateUrl: './store-view.component.html',
  styleUrls: ['./store-view.component.css']
})
export class StoreViewComponent implements OnInit {
  store = signal<Store | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  safeHeaderHtml = signal<SafeHtml | null>(null);
  safeContentHtml = signal<SafeHtml | null>(null);
  safeFooterHtml = signal<SafeHtml | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private storeService: StoreService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    const storeId = this.route.snapshot.paramMap.get('id');
    if (storeId) {
      this.loadStore(storeId);
    } else {
      this.errorMessage.set('Invalid store ID');
      this.isLoading.set(false);
    }
  }

  /**
   * Load store details
   */
  loadStore(storeId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.storeService.getStoreById(storeId).subscribe({
      next: (store: any) => {
        this.isLoading.set(false);
        this.store.set(store);

        // Sanitize HTML content for safe display
        if (store.headerHtml) {
          this.safeHeaderHtml.set(this.sanitizer.bypassSecurityTrustHtml(store.headerHtml));
        }
        if (store.contentHtml) {
          this.safeContentHtml.set(this.sanitizer.bypassSecurityTrustHtml(store.contentHtml));
        }
        if (store.footerHtml) {
          this.safeFooterHtml.set(this.sanitizer.bypassSecurityTrustHtml(store.footerHtml));
        }
      },
      error: (error: any) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Failed to load store. Please try again.');
        console.error('Error loading store:', error);
      }
    });
  }

  /**
   * Go back to store list
   */
  goBack(): void {
    this.router.navigate(['/merchant/dashboard']);
  }

  /**
   * Edit store settings
   */
  editStore(): void {
    const storeId = this.store()?.id;
    if (storeId) {
      this.router.navigate(['/merchant/stores', storeId, 'settings']);
    }
  }
}
