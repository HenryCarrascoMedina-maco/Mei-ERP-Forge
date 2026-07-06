import { HttpInterceptorFn } from '@angular/common/http';

const CORRELATION_ID_HEADER = 'X-Correlation-Id';

/** Generates an RFC4122-ish id without external dependencies. */
function generateCorrelationId(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return crypto.randomUUID().replace(/-/g, '');
  }
  return Math.random().toString(16).slice(2) + Date.now().toString(16);
}

/**
 * Attaches a unique correlation id to every outgoing request so frontend and backend
 * logs can be traced together. The backend echoes the same header back.
 */
export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  const cloned = req.clone({
    setHeaders: { [CORRELATION_ID_HEADER]: generateCorrelationId() },
  });
  return next(cloned);
};
