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
  // HTML Content fields for store customization
  headerHtml?: string;
  contentHtml?: string;
  footerHtml?: string;
}

export interface CreateStoreRequest {
  name: string;
  subdomain: string;
  businessName?: string;
  email: string;
  phone?: string;
  // HTML Content fields for store customization
  headerHtml?: string;
  contentHtml?: string;
  footerHtml?: string;
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
