import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthTokens,
  ChangePasswordRequest,
  ForgotPasswordRequest,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ResetPasswordRequest,
  User,
} from '../models/auth.model';
import { ApiResponse } from '../models/api-response.model';
import { ROLES } from '../constants/roles';

const TOKEN_KEY = 'sip_access_token';
const REFRESH_KEY = 'sip_refresh_token';
const EXPIRES_KEY = 'sip_expires_at';
const USER_KEY = 'sip_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly userSignal = signal<User | null>(this.loadUser());
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly isAdmin = computed(() => this.userSignal()?.roles.includes(ROLES.Admin) ?? false);
  readonly isStaff = computed(() => this.userSignal()?.roles.includes(ROLES.Staff) ?? false);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((res) => {
        if (res.success) this.persistSession(res.data);
      }),
      switchMap((res) => (res.success ? of(res.data) : throwError(() => new Error(res.message ?? 'Login failed'))))
    );
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/register`, request).pipe(
      tap((res) => {
        if (res.success) this.persistSession(res.data);
      }),
      switchMap((res) => (res.success ? of(res.data) : throwError(() => new Error(res.message ?? 'Registration failed'))))
    );
  }

  refreshToken(): Observable<LoginResponse | null> {
    const refreshToken = localStorage.getItem(REFRESH_KEY);
    if (!refreshToken) return of(null);
    return this.http
      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap((res) => {
          if (res.success) this.persistSession(res.data);
        }),
        switchMap((res) => (res.success ? of(res.data) : of(null))),
        catchError(() => of(null))
      );
  }

  loadCurrentUser(): Observable<User | null> {
    if (!this.getAccessToken()) return of(null);
    return this.http.get<ApiResponse<User>>(`${environment.apiUrl}/auth/me`).pipe(
      tap((res) => {
        if (res.success) {
          this.userSignal.set(res.data);
          localStorage.setItem(USER_KEY, JSON.stringify(res.data));
        }
      }),
      switchMap((res) => (res.success ? of(res.data) : of(null))),
      catchError(() => {
        this.clearSession();
        return of(null);
      })
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<{ message: string | null; resetToken?: string | null }> {
    return this.http
      .post<ApiResponse<{ resetToken?: string | null }>>(`${environment.apiUrl}/auth/forgot-password`, request)
      .pipe(
        switchMap((res) =>
          res.success
            ? of({ message: res.message, resetToken: res.data?.resetToken ?? null })
            : throwError(() => new Error(res.message ?? 'Request failed'))
        )
      );
  }

  resetPassword(request: ResetPasswordRequest): Observable<string | null> {
    return this.http.post<ApiResponse<unknown>>(`${environment.apiUrl}/auth/reset-password`, request).pipe(
      switchMap((res) => (res.success ? of(res.message) : throwError(() => new Error(res.message ?? 'Reset failed'))))
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<string | null> {
    return this.http.post<ApiResponse<unknown>>(`${environment.apiUrl}/auth/change-password`, request).pipe(
      switchMap((res) => (res.success ? of(res.message) : throwError(() => new Error(res.message ?? 'Change failed'))))
    );
  }

  logout(): void {
    this.clearSession();
    this.router.navigate(['/']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }

  getTokens(): AuthTokens | null {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshToken();
    const expiresAt = localStorage.getItem(EXPIRES_KEY);
    if (!accessToken || !refreshToken || !expiresAt) return null;
    return { accessToken, refreshToken, expiresAt };
  }

  private persistSession(data: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, data.accessToken);
    localStorage.setItem(REFRESH_KEY, data.refreshToken);
    localStorage.setItem(EXPIRES_KEY, data.expiresAt);
    localStorage.setItem(USER_KEY, JSON.stringify(data.user));
    this.userSignal.set(data.user);
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(EXPIRES_KEY);
    localStorage.removeItem(USER_KEY);
    this.userSignal.set(null);
  }

  private loadUser(): User | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as User;
    } catch {
      return null;
    }
  }
}
