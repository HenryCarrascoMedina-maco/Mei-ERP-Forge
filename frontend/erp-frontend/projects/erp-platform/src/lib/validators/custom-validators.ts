import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { emailValidator } from './email.validator';
import { passwordValidator, PasswordPolicyOptions } from './password.validator';
import { dateRangeValidator } from './date-range.validator';
import { fileValidator, FileValidatorOptions } from './file.validator';
import { duplicateValidator, DuplicateValidatorOptions } from './duplicate.validator';

/**
 * General-purpose reusable validators, plus a single entry point that also exposes the
 * specialized validators so feature code can import everything from one place:
 *
 * ```ts
 * import { CustomValidators } from '@shared/validators';
 * new FormControl('', [CustomValidators.notBlank(), CustomValidators.email()]);
 * ```
 */
export class CustomValidators {
  /** Required value that also rejects whitespace-only strings. Error key: `required`. */
  static notBlank(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      const isEmpty = value === null || value === undefined || String(value).trim() === '';
      return isEmpty ? { required: true } : null;
    };
  }

  /** Restricts to letters (incl. accents) and spaces. Empty passes. Error key: `onlyLetters`. */
  static onlyLetters(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;
      return /^[\p{L}\s]+$/u.test(String(value)) ? null : { onlyLetters: true };
    };
  }

  /**
   * Group-level validator that requires two controls to hold the same value (e.g. password
   * confirmation). The error is set on the group. Error key: `fieldsMismatch`.
   */
  static match(controlKey: string, matchingKey: string): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const control = group.get(controlKey);
      const matching = group.get(matchingKey);
      if (!control || !matching) return null;
      return control.value === matching.value ? null : { fieldsMismatch: { controlKey, matchingKey } };
    };
  }

  // --- Re-exported specialized validators (single import surface) -----------

  static email = emailValidator;

  static password = (options?: PasswordPolicyOptions): ValidatorFn => passwordValidator(options);

  static dateRange = (startKey: string, endKey: string): ValidatorFn => dateRangeValidator(startKey, endKey);

  static file = (options?: FileValidatorOptions): ValidatorFn => fileValidator(options);

  static duplicate = (
    existingValues: () => Array<string | number>,
    options?: DuplicateValidatorOptions,
  ): ValidatorFn => duplicateValidator(existingValues, options);
}
