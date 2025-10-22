import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap, map } from 'rxjs/operators';
import {
  Store,
  CreateStoreRequest,
  UpdateStoreRequest,
  StoreValidationResponse
} from '../models/store.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  // Signal-based state management for stores
  private storesSignal = signal<Store[]>([]);
  private currentStoreSignal = signal<Store | null>(null);

  readonly stores = this.storesSignal.asReadonly();
  readonly currentStore = this.currentStoreSignal.asReadonly();

  private readonly apiUrl = `${environment.apiUrl}/stores`;

  constructor(private http: HttpClient) {}

  /**
   * Create a new store
   */
  createStore(request: CreateStoreRequest): Observable<Store> {
    return this.http.post<Store>(this.apiUrl, request).pipe(
      tap(store => {
        // Add the new store to the stores list
        this.storesSignal.update(stores => [...stores, store]);
        this.currentStoreSignal.set(store);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Get all stores for the current merchant (owner)
   */
  getStoresByOwner(ownerId: string): Observable<Store[]> {
    return this.http.get<Store[]>(`${this.apiUrl}/by-owner/${ownerId}`).pipe(
      tap(stores => {
        this.storesSignal.set(stores);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Get a single store by ID
   */
  getStoreById(storeId: string): Observable<Store> {
    return this.http.get<Store>(`${this.apiUrl}/${storeId}`).pipe(
      tap(store => {
        this.currentStoreSignal.set(store);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Update an existing store
   */
  updateStore(storeId: string, request: UpdateStoreRequest): Observable<Store> {
    return this.http.put<Store>(`${this.apiUrl}/${storeId}`, request).pipe(
      tap(updatedStore => {
        // Update the store in the stores list
        this.storesSignal.update(stores =>
          stores.map(store => store.id === storeId ? updatedStore : store)
        );
        this.currentStoreSignal.set(updatedStore);
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Delete a store
   */
  deleteStore(storeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${storeId}`).pipe(
      tap(() => {
        // Remove the store from the stores list
        this.storesSignal.update(stores =>
          stores.filter(store => store.id !== storeId)
        );
        if (this.currentStoreSignal()?.id === storeId) {
          this.currentStoreSignal.set(null);
        }
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Check if subdomain is available
   * Returns true if available, false if taken
   */
  checkSubdomainAvailability(subdomain: string): Observable<boolean> {
    return this.http.get<StoreValidationResponse>(
      `${this.apiUrl}/validate-subdomain/${subdomain}`
    ).pipe(
      map(response => response.isValid),
      catchError(() => {
        // If validation endpoint doesn't exist, assume available
        return throwError(() => new Error('Unable to validate subdomain'));
      })
    );
  }

  /**
   * Clear the current store from state
   */
  clearCurrentStore(): void {
    this.currentStoreSignal.set(null);
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unexpected error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side or network error
      errorMessage = `Network error: ${error.error.message}`;
    } else {
      // Backend returned an unsuccessful response code
      switch (error.status) {
        case 400:
          errorMessage = error.error?.message || 'Invalid request. Please check your input.';
          break;
        case 401:
          errorMessage = 'Unauthorized. Please log in again.';
          break;
        case 403:
          errorMessage = 'Access denied. You do not have permission to perform this action.';
          break;
        case 404:
          errorMessage = 'Store not found.';
          break;
        case 409:
          errorMessage = error.error?.message || 'Subdomain already exists. Please choose another.';
          break;
        case 500:
          errorMessage = 'Server error. Please try again later.';
          break;
        default:
          errorMessage = error.error?.message || `Error: ${error.status}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}
