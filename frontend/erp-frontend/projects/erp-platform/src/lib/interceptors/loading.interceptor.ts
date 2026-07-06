import { HttpContextToken, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../services/loading.service';

/** Set this context token to `true` to keep a request out of the global loading indicator. */
export const SKIP_LOADING = new HttpContextToken<boolean>(() => false);

/**
 * Drives the global loading indicator: increments the counter when a request starts and
 * decrements it when the request settles. Background polling can opt out via {@link SKIP_LOADING}.
 */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_LOADING)) {
    return next(req);
  }

  const loading = inject(LoadingService);
  loading.show();

  return next(req).pipe(finalize(() => loading.hide()));
};
