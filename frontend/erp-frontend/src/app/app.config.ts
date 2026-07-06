import { ApplicationConfig, inject, provideAppInitializer, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { provideErpPlatform } from '@erp-platform/core';
import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { correlationIdInterceptor } from '@erp-platform/core';
import { authInterceptor } from '@erp-platform/core';
import { loadingInterceptor } from '@erp-platform/core';
import { errorInterceptor } from '@erp-platform/core';
import { AuthService } from '@erp-platform/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    // Supply the library's runtime config (decouples @erp-platform/core from the app's environment).
    provideErpPlatform({ apiUrl: environment.apiUrl }),
    provideHttpClient(
      // Order matters: authInterceptor is LAST (innermost) so it handles 401 / refresh-on-401
      // before the error interceptor surfaces the failure.
      withInterceptors([
        correlationIdInterceptor,
        loadingInterceptor,
        errorInterceptor,
        authInterceptor,
      ]),
    ),
    // On startup, if a session token is present, hydrate the current user + permissions from /me
    // (so guards/permissions work after a browser reload). An expired access token is transparently
    // refreshed by the auth interceptor; if the refresh also fails, the session is cleared and the
    // user is redirected to /login. No auto-login: unauthenticated users land on the login screen.
    provideAppInitializer(() => {
      const auth = inject(AuthService);
      if (!auth.getToken()) {
        return Promise.resolve();
      }
      return firstValueFrom(auth.loadCurrentUser().pipe(catchError(() => of(null))));
    }),
  ],
};
