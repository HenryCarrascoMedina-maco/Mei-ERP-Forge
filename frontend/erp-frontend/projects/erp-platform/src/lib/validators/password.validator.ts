import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Configurable password strength rules. Mirrors a typical backend `PasswordPolicy`. */
export interface PasswordPolicyOptions {
  minLength?: number;
  requireUppercase?: boolean;
  requireLowercase?: boolean;
  requireDigit?: boolean;
  requireSpecial?: boolean;
}

const DEFAULT_POLICY: Required<PasswordPolicyOptions> = {
  minLength: 8,
  requireUppercase: true,
  requireLowercase: true,
  requireDigit: true,
  requireSpecial: true,
};

/**
 * Validates password strength against a configurable policy. Empty values pass (pair with
 * `Validators.required`). Returns a `password` error whose value lists the unmet rules:
 * `{ password: { minLength, uppercase, lowercase, digit, special } }`.
 *
 * Usage: `new FormControl('', [passwordValidator({ minLength: 10 })])`
 */
export function passwordValidator(options: PasswordPolicyOptions = {}): ValidatorFn {
  const policy = { ...DEFAULT_POLICY, ...options };

  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) {
      return null;
    }

    const failures: Record<string, boolean> = {};
    if (value.length < policy.minLength) failures['minLength'] = true;
    if (policy.requireUppercase && !/[A-Z]/.test(value)) failures['uppercase'] = true;
    if (policy.requireLowercase && !/[a-z]/.test(value)) failures['lowercase'] = true;
    if (policy.requireDigit && !/\d/.test(value)) failures['digit'] = true;
    if (policy.requireSpecial && !/[^A-Za-z0-9]/.test(value)) failures['special'] = true;

    return Object.keys(failures).length > 0 ? { password: failures } : null;
  };
}
