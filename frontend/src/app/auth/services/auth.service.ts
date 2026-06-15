import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

const TOKEN_KEY = 'loan_app_token';
const USER_KEY = 'loan_app_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

  private readonly _token = signal<string | null>(this.getStoredToken());
  private readonly _username = signal<string | null>(this.getStoredUser());

  readonly token = this._token.asReadonly();
  readonly username = this._username.asReadonly();
  readonly isAuthenticated = computed(() => !!this._token());

  async register(request: RegisterRequest): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${this.baseUrl}/register`, request),
    );
    this.storeAuth(response);
  }

  async login(request: LoginRequest): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${this.baseUrl}/login`, request),
    );
    this.storeAuth(response);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._token.set(null);
    this._username.set(null);
    void this.router.navigate(['/login']);
  }

  private storeAuth(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, response.username);
    this._token.set(response.token);
    this._username.set(response.username);
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private getStoredUser(): string | null {
    return localStorage.getItem(USER_KEY);
  }
}
