import { ValidatorFn } from '@angular/forms';
import { SelectOption } from '../../../models';

/** Supported input control types. */
export type FormFieldType =
  | 'text'
  | 'number'
  | 'email'
  | 'password'
  | 'textarea'
  | 'select'
  | 'multiselect'
  | 'autocomplete'
  | 'date'
  | 'checkbox'
  | 'switch'
  | 'file';

/**
 * Defines a single field within the generic dynamic form.
 *
 * @typeParam T The form model type, used to type the value-driven predicates.
 */
export interface FormFieldConfig<T = Record<string, unknown>> {
  /** Form control name. */
  key: string;
  label: string;
  type: FormFieldType;
  placeholder?: string;
  /** Angular reactive validators applied to the control. */
  validators?: ValidatorFn[];
  /** Options for select / autocomplete fields. */
  options?: SelectOption[];
  /** Static disabled state, or a predicate over the current form value. */
  disabled?: boolean | ((value: Partial<T>) => boolean);
  /** Static hidden state, or a predicate over the current form value (conditional visibility). */
  hidden?: boolean | ((value: Partial<T>) => boolean);
  defaultValue?: unknown;
  /** Number of grid columns the field spans (1-12). Defaults to 12 (full width). */
  colSpan?: number;
  /** Hint text rendered under the field. */
  hint?: string;
}
