/**
 * Standard option model for selects, dropdowns and autocompletes.
 */
export interface SelectOption<T = string | number> {
  value: T;
  label: string;
  disabled?: boolean;
  icon?: string;
  /** Optional grouping key for grouped selects. */
  group?: string;
}
