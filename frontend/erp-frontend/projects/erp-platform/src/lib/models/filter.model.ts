import { PaginationRequest } from './pagination-request.model';

/**
 * Base filter request. Concrete modules extend this with their own typed filter fields
 * while inheriting pagination, search and sorting. Mirrors the backend `FilterParams`.
 */
export interface FilterRequest extends PaginationRequest {
  isActive?: boolean | null;
  createdFrom?: string | null;
  createdTo?: string | null;
}
