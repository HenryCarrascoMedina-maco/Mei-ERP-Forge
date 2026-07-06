import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export interface DuplicateValidatorOptions {
  caseInsensitive?: boolean;
  /** Trim before comparing. Defaults to true. */
  trim?: boolean;
}

/**
 * Validates that a control's value is not already present in a set of existing values
 * (e.g. uniqueness of a code/name against already-loaded records). The existing values are
 * provided lazily so the validator always sees the current list. Empty values pass.
 * Error key: `duplicate`.
 *
 * Usage:
 * ```ts
 * new FormControl('', [duplicateValidator(() => this.existingCodes(), { caseInsensitive: true })])
 * ```
 */
export function duplicateValidator(
  existingValues: () => Array<string | number>,
  options: DuplicateValidatorOptions = {},
): ValidatorFn {
  const { caseInsensitive = false, trim = true } = options;

  const normalize = (v: unknown): string => {
    let s = String(v ?? '');
    if (trim) s = s.trim();
    if (caseInsensitive) s = s.toLowerCase();
    return s;
  };

  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const target = normalize(value);
    const exists = existingValues().some((v) => normalize(v) === target);

    return exists ? { duplicate: true } : null;
  };
}
