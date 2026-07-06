import { Sort } from '../../../models';

/**
 * Behavioral configuration for the generic table.
 */
export interface TableConfig {
  showPagination?: boolean;
  showSelection?: boolean;
  /** Allow selecting more than one row. Requires showSelection. */
  multiSelect?: boolean;
  pageSizeOptions?: number[];
  defaultSort?: Sort;
  loading?: boolean;
  emptyMessage?: string;
  /** Emit rowClick events when a row is clicked. */
  clickableRows?: boolean;
  /** Column key used to track rows for selection / change detection. */
  trackBy?: string;
}

/** Default table configuration applied when fields are omitted. */
export const DEFAULT_TABLE_CONFIG: Required<Pick<TableConfig, 'showPagination' | 'pageSizeOptions' | 'emptyMessage'>> = {
  showPagination: true,
  pageSizeOptions: [10, 25, 50, 100],
  emptyMessage: 'No records found.',
};
