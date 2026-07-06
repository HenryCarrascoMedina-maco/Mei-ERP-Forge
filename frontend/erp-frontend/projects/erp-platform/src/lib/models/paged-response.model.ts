/**
 * Pagination metadata. Mirrors the backend `PaginationMeta`.
 */
export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

/**
 * Standard paginated response envelope. Mirrors the backend `PagedResponse<T>`.
 */
export interface PagedResponse<T> {
  success: boolean;
  message?: string | null;
  data: T[];
  meta: PaginationMeta;
  correlationId?: string | null;
}
