/**
 * Sorting direction. String values match the backend `SortDirection` enum names.
 */
export type SortDirection = 'Ascending' | 'Descending';

/**
 * Sorting configuration for a list/table.
 */
export interface Sort {
  sortBy: string;
  sortDirection: SortDirection;
}
