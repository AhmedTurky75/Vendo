import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { StoreService } from '../../../core/services/store.service';
import { Store, UpdateStoreRequest } from '../../../core/models/store.model';

@Component({
  selector: 'app-store-settings',
  standalone : false,
  templateUrl: './store-settings.component.html',
  styleUrls: ['./store-settings.component.css']
})
export class StoreSettingsComponent implements OnInit {
  settingsForm: FormGroup;
  store = signal<Store | null>(null);
  isLoading = signal(false);
  isSaving = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isCheckingSubdomain = signal(false);
  subdomainPreview = signal<string>('');
  storeId: string = '';
  originalSubdomain: string = '';

  constructor(
    private fb: FormBuilder,
    private storeService: StoreService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.settingsForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      subdomain: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(63), this.subdomainValidator]],
      businessName: ['', [Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.pattern(/^[\d\s+()-]+$/)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    // Get store ID from route params
    this.route.params.subscribe(params => {
      this.storeId = params['id'];
      if (this.storeId) {
        this.loadStore();
      }
    });

    // Set up real-time subdomain validation and preview
    this.settingsForm.get('subdomain')?.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(subdomain => {
      if (subdomain && subdomain.length >= 3 && subdomain !== this.originalSubdomain) {
        this.updateSubdomainPreview(subdomain);
        this.checkSubdomainAvailability(subdomain);
      } else if (subdomain) {
        this.updateSubdomainPreview(subdomain);
      } else {
        this.subdomainPreview.set('');
      }
    });
  }

  /**
   * Load store details
   */
  loadStore(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.storeService.getStoreById(this.storeId).subscribe({
      next: (store) => {
        this.isLoading.set(false);
        this.store.set(store);
        this.originalSubdomain = store.subdomain;

        // Populate form with store data
        this.settingsForm.patchValue({
          name: store.name,
          subdomain: store.subdomain,
          businessName: store.businessName || '',
          email: store.email,
          phone: store.phone || '',
          isActive: store.isActive
        });

        this.updateSubdomainPreview(store.subdomain);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Failed to load store details. Please try again.');
        console.error('Error loading store:', error);
      }
    });
  }

  /**
   * Custom validator for subdomain format
   */
  private subdomainValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null;
    }

    const subdomainPattern = /^[a-z0-9]([a-z0-9-]*[a-z0-9])?$/;
    const valid = subdomainPattern.test(control.value);

    return valid ? null : { invalidSubdomain: true };
  }

  /**
   * Update the subdomain preview
   */
  private updateSubdomainPreview(subdomain: string): void {
    this.subdomainPreview.set(`Your store will be at: localhost:4200/stores/${subdomain}`);
  }

  /**
   * Check if subdomain is available
   */
  private checkSubdomainAvailability(subdomain: string): void {
    const subdomainControl = this.settingsForm.get('subdomain');

    if (subdomainControl?.errors && subdomainControl.errors['invalidSubdomain']) {
      return;
    }

    this.isCheckingSubdomain.set(true);

    this.storeService.checkSubdomainAvailability(subdomain).subscribe({
      next: (isAvailable) => {
        this.isCheckingSubdomain.set(false);

        if (!isAvailable) {
          subdomainControl?.setErrors({ subdomainTaken: true });
        } else {
          const errors = subdomainControl?.errors;
          if (errors && errors['subdomainTaken']) {
            delete errors['subdomainTaken'];
            subdomainControl?.setErrors(Object.keys(errors).length > 0 ? errors : null);
          }
        }
      },
      error: () => {
        this.isCheckingSubdomain.set(false);
      }
    });
  }

  /**
   * Save store settings
   */
  onSubmit(): void {
    if (this.settingsForm.invalid) {
      this.settingsForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const request: UpdateStoreRequest = {
      name: this.settingsForm.value.name,
      subdomain: this.settingsForm.value.subdomain.toLowerCase(),
      businessName: this.settingsForm.value.businessName || undefined,
      email: this.settingsForm.value.email,
      phone: this.settingsForm.value.phone || undefined,
      isActive: this.settingsForm.value.isActive
    };

    this.storeService.updateStore(this.storeId, request).subscribe({
      next: (updatedStore) => {
        this.isSaving.set(false);
        this.successMessage.set('Store settings updated successfully!');
        this.store.set(updatedStore);
        this.originalSubdomain = updatedStore.subdomain;

        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (error) => {
        this.isSaving.set(false);
        this.errorMessage.set(error.message || 'Failed to update store settings. Please try again.');
        console.error('Error updating store:', error);
      }
    });
  }

  /**
   * Navigate back to dashboard
   */
  goToDashboard(): void {
    this.router.navigate(['/merchant/dashboard']);
  }

  /**
   * Delete store (with confirmation)
   */
  deleteStore(): void {
    if (!confirm('Are you sure you want to delete this store? This action cannot be undone.')) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.storeService.deleteStore(this.storeId).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/merchant/dashboard']);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Failed to delete store. Please try again.');
        console.error('Error deleting store:', error);
      }
    });
  }

  // Form field getters
  get name() {
    return this.settingsForm.get('name');
  }

  get subdomain() {
    return this.settingsForm.get('subdomain');
  }

  get businessName() {
    return this.settingsForm.get('businessName');
  }

  get email() {
    return this.settingsForm.get('email');
  }

  get phone() {
    return this.settingsForm.get('phone');
  }

  get isActive() {
    return this.settingsForm.get('isActive');
  }
}
