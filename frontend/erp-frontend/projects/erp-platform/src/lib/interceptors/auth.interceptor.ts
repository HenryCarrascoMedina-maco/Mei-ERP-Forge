import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/** Set this context token to `true` on a request to skip attaching the bearer token / refresh. */
export const SKIP_AUTH = new HttpContextToken<boolean>(() => false);

/** Auth endpoints that must never trigger a refresh-on-401 (prevents refresh loops). */
const NON_REFRESHABLE = ['/auth/token', '/auth/refresh', '/auth/logout'];

/**
 * Attaches the JWT bearer token to outgoing requests and transparently recovers from expiry:
 * on a 401 it performs a single-flight refresh (see {@link AuthService.refresh}) and retries the
 * original request with the new token. If the refresh fails, the session is cleared and the user is
 * redirected to `/login`. Requests flagged with {@link SKIP_AUTH} and the auth endpoints themselves
 * are never refreshed. Place this interceptor LAST (innermost) so it handles 401s before the global
 * error interceptor.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const skip = req.context.get(SKIP_AUTH);
  const token = auth.getToken();
  const outgoing = !skip && token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(outgoing).pipe(
    catchError((error: HttpErrorResponse) => {
      const canRefresh =
        error.status === 401 &&
        !skip &&
        !NON_REFRESHABLE.some((path) => req.url.includes(path)) &&
        !!auth.getRefreshToken();

      if (!canRefresh) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap((newToken) =>
          newToken
            ? next(req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }))
            : throwError(() => error)),
        catchError((refreshError) => {
          auth.clearSession();
          void router.navigate(['/login']);
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
