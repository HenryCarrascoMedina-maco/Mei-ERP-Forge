/**
 * Error tied to a specific imported row. Mirrors the backend `ImportRowError`.
 */
export interface ImportRowError {
  /** 1-based data row number (header excluded). */
  row: number;
  column?: string | null;
  message: string;
}

/**
 * Outcome of an import operation. Mirrors the backend `ImportResult<T>`.
 */
export interface ImportResult<T = unknown> {
  totalRows: number;
  successfulRows: number;
  failedRows: number;
  items: T[];
  errors: ImportRowError[];
  hasErrors: boolean;
}
