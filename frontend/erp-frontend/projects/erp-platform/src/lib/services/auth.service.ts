import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, finalize, map, of, shareReplay, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { PermissionService } from './permission.service';

const TOKEN_KEY = 'erp.access_token';
const REFRESH_KEY = 'erp.refresh_token';

/** Authentication result returned by `POST /api/auth/token` and `/refresh`. */
export interface AuthResult {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  roles: string[];
  permissions: string[];
}

/** Shape of `GET /api/auth/me`. */
export interface CurrentUser {
  userId: string;
  email: string;
  userName: string;
  roles: string[];
  permissions: string[];
}

/**
 * Real, backend-backed authentication: credential login, session/token storage, current-user
 * hydration, single-flight refresh-token rotation and logout. Tokens are issued by the backend
 * Identity layer; permissions/roles flow into {@link PermissionService} so guards and the
 * `*appHasPermission` directive work unchanged.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly storage = inject(StorageService);
  private readonly permissions = inject(PermissionService);

  private readonly token = signal<string | null>(this.storage.get<string>(TOKEN_KEY));
  private readonly user = signal<CurrentUser | null>(null);

  /** Reactive authentication flag (a token is present). */
  readonly isAuthenticated = computed(() => this.token() !== null);

  /** The hydrated current user (after login or /me), or null. */
  readonly currentUser = computed(() => this.user());

  /** In-flight refresh, shared so concurrent 401s trigger a single refresh call. */
  private refreshInFlight: Observable<string | null> | null = null;

  getToken(): string | null {
    return this.token();
  }

  getRefreshToken(): string | null {
    return this.storage.get<string>(REFRESH_KEY);
  }

  /** Authenticates against `POST /api/auth/token` and hydrates the session + permissions. */
  login(userName: string, password: string): Observable<AuthResult | null> {
    return this.api
      .post<AuthResult>('auth/token', { userName, password })
      .pipe(tap((res) => res && this.applyAuth(res)));
  }

  /** Loads the current user from `GET /api/auth/me` and hydrates roles/permissions (app bootstrap). */
  loadCurrentUser(): Observable<CurrentUser | null> {
    return this.api.get<CurrentUser>('auth/me').pipe(
      tap((me) => {
        if (me) {
          this.user.set(me);
          this.permissions.setRoles(me.roles ?? []);
          this.permissions.setPermissions(me.permissions ?? []);
        }
      }),
    );
  }

  /** Rotates the refresh token (single-flight). Resolves to the new access token, or errors. */
  refresh(): Observable<string | null> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available.'));
    }

    this.refreshInFlight = this.api.post<AuthResult>('auth/refresh', { refreshToken }).pipe(
      map((res) => {
        if (!res) {
          throw new Error('Refresh failed.');
        }
        this.applyAuth(res);
        return res.accessToken;
      }),
      finalize(() => (this.refreshInFlight = null)),
      shareReplay(1),
    );

    return this.refreshInFlight;
  }

  /** Revokes the refresh token server-side (`POST /api/auth/logout`) and clears the local session. */
  logout(): Observable<unknown> {
    const refreshToken = this.getRefreshToken();
    const call = refreshToken ? this.api.post('auth/logout', { refreshToken }) : of(null);
    return call.pipe(
      catchError(() => of(null)),
      finalize(() => this.clearSession()),
    );
  }

  setSession(accessToken: string, refreshToken?: string): void {
    this.storage.set(TOKEN_KEY, accessToken);
    if (refreshToken) {
      this.storage.set(REFRESH_KEY, refreshToken);
    }
    this.token.set(accessToken);
  }

  /** Clears all local auth state (no server call). Used by the interceptor when refresh fails. */
  clearSession(): void {
    this.storage.remove(TOKEN_KEY);
    this.storage.remove(REFRESH_KEY);
    this.token.set(null);
    this.user.set(null);
    this.permissions.clear();
  }

  private applyAuth(res: AuthResult): void {
    this.setSession(res.accessToken, res.refreshToken);
    this.permissions.setRoles(res.roles ?? []);
    this.permissions.setPermissions(res.permissions ?? []);
  }
}
