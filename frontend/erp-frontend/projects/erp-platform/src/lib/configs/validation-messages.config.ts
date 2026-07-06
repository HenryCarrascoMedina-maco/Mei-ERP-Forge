/** A validation message is either a static string or a function of the error payload. */
export type ValidationMessage = string | ((error: unknown) => string);

/**
 * Standard, centralized validation messages keyed by Angular/​custom error keys. Use with
 * `FormUtil.getErrorMessage` to render consistent feedback across forms.
 */
export const VALIDATION_MESSAGES: Record<string, ValidationMessage> = {
  required: 'This field is required.',
  email: 'Enter a valid email address.',
  min: (e) => `Value must be at least ${(e as { min: number }).min}.`,
  max: (e) => `Value must be at most ${(e as { max: number }).max}.`,
  minlength: (e) => `Must be at least ${(e as { requiredLength: number }).requiredLength} characters.`,
  maxlength: (e) => `Must be at most ${(e as { requiredLength: number }).requiredLength} characters.`,
  pattern: 'The value has an invalid format.',
  onlyLetters: 'Only letters are allowed.',
  password: 'The password does not meet the required policy.',
  dateRange: 'The start date must be before the end date.',
  duplicate: 'This value already exists.',
  fieldsMismatch: 'The values do not match.',
  file: 'The selected file is not valid.',
};

export const DEFAULT_VALIDATION_MESSAGE = 'Invalid value.';
