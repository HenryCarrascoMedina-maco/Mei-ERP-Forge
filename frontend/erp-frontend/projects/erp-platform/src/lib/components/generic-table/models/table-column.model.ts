/** Supported built-in cell renderings. */
export type ColumnType = 'text' | 'number' | 'date' | 'datetime' | 'currency' | 'boolean' | 'badge' | 'custom';

export type ColumnAlign = 'left' | 'center' | 'right';

/**
 * Defines a single column of the generic table.
 *
 * @typeParam T Row data type.
 */
export interface TableColumn<T = Record<string, unknown>> {
  /** Property key on the row, or an arbitrary id for custom columns. */
  key: string;
  /** Header label. */
  label: string;
  type?: ColumnType;
  sortable?: boolean;
  /** Whether the column is visible. Defaults to true. */
  visible?: boolean;
  width?: string;
  /** Pin the column to the start when scrolling horizontally. */
  sticky?: boolean;
  align?: ColumnAlign;
  /** Optional cell formatter; receives the cell value and the full row. */
  formatter?: (value: unknown, row: T) => string;
}
