import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ErrorHandlerService } from '../services/error-handler.service';
import { ToastService } from '../services/toast.service';

/**
 * Global HTTP error handler. Converts failures into user-friendly toasts via
 * {@link ErrorHandlerService} and re-throws so callers can still react. 401s are owned by the
 * auth interceptor (refresh-on-401, then redirect to /login on failure), so they are not toasted
 * here to avoid noise during silent token refresh.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorHandler = inject(ErrorHandlerService);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        toast.error(errorHandler.getMessage(error));
      }
      return throwError(() => error);
    }),
  );
};
