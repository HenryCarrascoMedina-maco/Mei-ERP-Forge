/**
 * Default pagination settings used across list/table screens. Centralized so every module
 * paginates consistently.
 */
export const PAGINATION_CONFIG = {
  defaultPage: 1,
  defaultPageSize: 10,
  pageSizeOptions: [10, 25, 50, 100] as number[],
} as const;
