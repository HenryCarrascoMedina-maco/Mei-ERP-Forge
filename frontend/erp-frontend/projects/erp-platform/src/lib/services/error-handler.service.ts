import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorResponse } from '../models';

/**
 * Translates backend / transport errors into readable, user-facing messages.
 * Understands the backend's standard `ErrorResponse` / `ValidationErrorResponse` shape.
 */
@Injectable({ providedIn: 'root' })
export class ErrorHandlerService {
  /** Returns a single human-readable message for any HTTP error. */
  getMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to reach the server. Please check your connection.';
    }

    const body = error.error as ApiErrorResponse | undefined;

    const validationMessages = this.getValidationMessages(error);
    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }

    if (body?.message) {
      return body.message;
    }

    switch (error.status) {
      case 401:
        return 'Your session has expired. Please sign in again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 409:
        return 'The resource conflicts with existing data.';
      default:
        return 'An unexpected error occurred. Please try again later.';
    }
  }

  /** Returns the flattened list of per-field validation messages, if any. */
  getValidationMessages(error: HttpErrorResponse): string[] {
    const body = error.error as ApiErrorResponse | undefined;
    if (!body?.errors) {
      return [];
    }
    return Object.values(body.errors).flat();
  }

  /** Returns the field-keyed validation map, for binding errors back onto a form. */
  getFieldErrors(error: HttpErrorResponse): Record<string, string[]> {
    const body = error.error as ApiErrorResponse | undefined;
    return body?.errors ?? {};
  }
}
