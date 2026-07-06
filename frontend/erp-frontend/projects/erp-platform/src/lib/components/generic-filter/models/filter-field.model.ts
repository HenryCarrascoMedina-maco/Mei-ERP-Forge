import { SelectOption } from '../../../models';

/** Supported filter control types. */
export type FilterFieldType = 'text' | 'select' | 'multiselect' | 'date' | 'daterange' | 'boolean';

/**
 * Defines a single filter control in the generic filter bar.
 */
export interface FilterFieldConfig {
  /** Key emitted in the filter values object. For 'daterange', `${key}From` / `${key}To` are emitted. */
  key: string;
  label: string;
  type: FilterFieldType;
  placeholder?: string;
  /** Options for select / multiselect / boolean overrides. */
  options?: SelectOption[];
  /** Grid columns (1-12) the field spans. Defaults to 3. */
  colSpan?: number;
}

/** Flat object of applied filter values (nulls/empties removed). */
export type FilterValues = Record<string, string | number | boolean | null>;
