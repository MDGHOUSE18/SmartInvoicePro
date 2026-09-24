export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface User {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phone?: string;
  roles: string[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}
