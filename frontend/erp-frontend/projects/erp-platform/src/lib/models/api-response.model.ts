/**
 * Standard API response envelope. Mirrors the backend `ApiResponse<T>`.
 */
export interface ApiResponse<T = unknown> {
  success: boolean;
  message?: string | null;
  data?: T | null;
  correlationId?: string | null;
}

/**
 * Standard error envelope returned by the backend global exception handler.
 * Mirrors `ErrorResponse` / `ValidationErrorResponse`.
 */
export interface ApiErrorResponse {
  success: false;
  message: string;
  statusCode: number;
  errorCode?: string | null;
  correlationId?: string | null;
  detail?: string | null;
  /** Present only for validation errors: field -> messages. */
  errors?: Record<string, string[]>;
}
