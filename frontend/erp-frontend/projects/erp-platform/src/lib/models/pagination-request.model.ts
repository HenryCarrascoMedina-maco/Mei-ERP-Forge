import { SortDirection } from './sort.model';

/**
 * Request parameters for paginated, searchable and sortable list endpoints.
 * Mirrors the backend `PaginationParams`.
 */
export interface PaginationRequest {
  page: number;
  pageSize: number;
  search?: string | null;
  sortBy?: string | null;
  sortDirection?: SortDirection;
}

/** Sensible default request for a first page load. */
export const DEFAULT_PAGINATION_REQUEST: PaginationRequest = {
  page: 1,
  pageSize: 10,
};
