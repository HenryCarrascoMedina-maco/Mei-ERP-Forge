import { FormFieldConfig } from './form-field-config.model';

/**
 * Groups a set of form fields under a titled, optionally collapsible section.
 */
export interface FormSection<T = Record<string, unknown>> {
  title?: string;
  description?: string;
  fields: FormFieldConfig<T>[];
  collapsible?: boolean;
  /** Initial collapsed state when collapsible. */
  collapsed?: boolean;
}

/** Operating mode for the generic form. */
export type FormMode = 'create' | 'edit' | 'view';
