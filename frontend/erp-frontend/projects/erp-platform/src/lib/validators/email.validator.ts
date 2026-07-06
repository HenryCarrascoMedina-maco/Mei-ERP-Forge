import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Pragmatic email pattern: local@domain.tld, no spaces, single @.
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

/**
 * Validates that a control holds a well-formed email address. Empty values pass (combine with
 * `Validators.required` when the field is mandatory). Error key: `email`.
 *
 * Usage: `new FormControl('', [emailValidator()])`
 */
export function emailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (value === null || value === undefined || value === '') {
      return null;
    }
    return EMAIL_PATTERN.test(String(value)) ? null : { email: true };
  };
}
