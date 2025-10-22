export enum UserRole {
  Admin = 'admin',
  Merchant = 'merchant',
  Customer = 'customer'
}

export interface User {
  id: string;
  email: string;
  role: UserRole;
  tenantId?: string;
  firstName?: string;
  lastName?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  role: UserRole;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
  expiresIn: number;
}
