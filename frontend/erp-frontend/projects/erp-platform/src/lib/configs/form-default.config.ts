import { MatFormFieldAppearance } from '@angular/material/form-field';

/**
 * Default configuration applied to dynamic forms (labels, field appearance, grid width).
 */
export const FORM_DEFAULTS = {
  appearance: 'outline' as MatFormFieldAppearance,
  /** Default number of grid columns (out of 12) a field spans. */
  defaultColSpan: 12,
  submitLabel: 'Save',
  cancelLabel: 'Cancel',
} as const;
