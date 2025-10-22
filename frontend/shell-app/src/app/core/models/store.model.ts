export interface Store {
  id: string;
  name: string;
  subdomain: string;
  businessName?: string;
  email: string;
  phone?: string;
  ownerId: string;
  tenantId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateStoreRequest {
  name: string;
  subdomain: string;
  businessName?: string;
  email: string;
  phone?: string;
}

export interface UpdateStoreRequest {
  name?: string;
  subdomain?: string;
  businessName?: string;
  email?: string;
  phone?: string;
  isActive?: boolean;
}

export interface StoreValidationResponse {
  isValid: boolean;
  message?: string;
}
