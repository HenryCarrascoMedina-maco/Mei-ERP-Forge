import { Sort } from '../../../models';

/** Emitted when the user changes page or page size. */
export interface PageChangeEvent {
  page: number;
  pageSize: number;
}

/** Emitted when the user sorts a column. */
export type SortChangeEvent = Sort;
