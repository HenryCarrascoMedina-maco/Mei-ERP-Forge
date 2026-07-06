import { AbstractControl, FormGroup } from '@angular/forms';
import { DEFAULT_VALIDATION_MESSAGE, VALIDATION_MESSAGES } from '../configs/validation-messages.config';

/**
 * Reactive form helper functions.
 */
export class FormUtil {
  /** Marks every control as touched and dirty so validation messages appear. */
  static markAllTouched(group: FormGroup): void {
    Object.values(group.controls).forEach((control) => {
      control.markAsTouched();
      control.markAsDirty();
      if (control instanceof FormGroup) {
        this.markAllTouched(control);
      }
    });
  }

  /** Returns only the values of controls the user has modified. */
  static getDirtyValues(group: FormGroup): Record<string, unknown> {
    const dirty: Record<string, unknown> = {};
    Object.entries(group.controls).forEach(([key, control]) => {
      if (control.dirty) {
        dirty[key] = control instanceof FormGroup ? this.getDirtyValues(control) : control.value;
      }
    });
    return dirty;
  }

  /**
   * Applies backend validation errors (field → messages) onto the matching controls so they
   * display inline. Unmatched fields are returned for global handling.
   */
  static applyServerErrors(group: FormGroup, errors: Record<string, string[]>): string[] {
    const unmatched: string[] = [];
    Object.entries(errors).forEach(([field, messages]) => {
      const control = group.get(field);
      if (control) {
        control.setErrors({ server: messages[0] });
        control.markAsTouched();
      } else {
        unmatched.push(...messages);
      }
    });
    return unmatched;
  }

  /** Resolves a human-readable message for a control's first error. */
  static getErrorMessage(control: AbstractControl | null): string | null {
    if (!control || !control.errors) {
      return null;
    }
    const [key, payload] = Object.entries(control.errors)[0];

    // Server-provided message takes precedence.
    if (key === 'server' && typeof payload === 'string') {
      return payload;
    }

    const message = VALIDATION_MESSAGES[key];
    if (!message) {
      return DEFAULT_VALIDATION_MESSAGE;
    }
    return typeof message === 'function' ? message(payload) : message;
  }
}
