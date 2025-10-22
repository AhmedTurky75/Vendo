import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { StoreService } from '../../core/services/store.service';
import { AuthService } from '../../core/services/auth.service';
import { CreateStoreRequest } from '../../core/models/store.model';

@Component({
  selector: 'app-store-creation',
  standalone: false,
  templateUrl: './store-creation.component.html',
  styleUrls: ['./store-creation.component.css']
})
export class StoreCreationComponent implements OnInit {
  storeForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isCheckingSubdomain = signal(false);
  subdomainPreview = signal<string>('');

  constructor(
    private fb: FormBuilder,
    private storeService: StoreService,
    private authService: AuthService,
    private router: Router
  ) {
    this.storeForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      subdomain: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(63), this.subdomainValidator]],
      businessName: ['', [Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.pattern(/^[\d\s+()-]+$/)]]
    });
  }

  ngOnInit(): void {
    // Pre-fill email from current user
    const currentUser = this.authService.currentUser();
    if (currentUser?.email) {
      this.storeForm.patchValue({ email: currentUser.email });
    }

    // Set up real-time subdomain validation and preview
    this.storeForm.get('subdomain')?.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(subdomain => {
      if (subdomain && subdomain.length >= 3) {
        this.updateSubdomainPreview(subdomain);
        this.checkSubdomainAvailability(subdomain);
      } else {
        this.subdomainPreview.set('');
      }
    });
  }

  /**
   * Custom validator for subdomain format
   * Must be alphanumeric with optional hyphens, cannot start/end with hyphen
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
   * Check if subdomain is available (real-time validation)
   */
  private checkSubdomainAvailability(subdomain: string): void {
    const subdomainControl = this.storeForm.get('subdomain');

    // Only check if subdomain format is valid
    if (subdomainControl?.errors && subdomainControl.errors['invalidSubdomain']) {
      return;
    }

    this.isCheckingSubdomain.set(true);

    this.storeService.checkSubdomainAvailability(subdomain).pipe(
      catchError(() => {
        // If validation endpoint fails, assume available
        return of(true);
      })
    ).subscribe((isAvailable: boolean) => {
      this.isCheckingSubdomain.set(false);

      if (!isAvailable) {
        subdomainControl?.setErrors({ subdomainTaken: true });
      } else {
        // Clear the subdomainTaken error if it exists
        const errors = subdomainControl?.errors;
        if (errors && errors['subdomainTaken']) {
          delete errors['subdomainTaken'];
          subdomainControl?.setErrors(Object.keys(errors).length > 0 ? errors : null);
        }
      }
    });
  }

  /**
   * Submit the store creation form
   */
  onSubmit(): void {
    if (this.storeForm.invalid) {
      this.storeForm.markAllAsTouched();
      return;
    }

    const currentUser = this.authService.currentUser();
    if (!currentUser) {
      this.errorMessage.set('User not authenticated. Please log in again.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const request: CreateStoreRequest = {
      name: this.storeForm.value.name,
      subdomain: this.storeForm.value.subdomain.toLowerCase(),
      businessName: this.storeForm.value.businessName || undefined,
      email: this.storeForm.value.email,
      phone: this.storeForm.value.phone || undefined
    };

    this.storeService.createStore(request).subscribe({
      next: (store :any) => {
        this.isLoading.set(false);
        this.successMessage.set('Store created successfully!');

        // Navigate to dashboard after a short delay
        setTimeout(() => {
          this.router.navigate(['/merchant/dashboard']);
        }, 1500);
      },
      error: (error :any) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Failed to create store. Please try again.');
        console.error('Store creation error:', error);
      }
    });
  }

  /**
   * Navigate to dashboard (if user already has stores)
   */
  goToDashboard(): void {
    this.router.navigate(['/merchant/dashboard']);
  }

  // Form field getters for easier access in template
  get name() {
    return this.storeForm.get('name');
  }

  get subdomain() {
    return this.storeForm.get('subdomain');
  }

  get businessName() {
    return this.storeForm.get('businessName');
  }

  get email() {
    return this.storeForm.get('email');
  }

  get phone() {
    return this.storeForm.get('phone');
  }
}
