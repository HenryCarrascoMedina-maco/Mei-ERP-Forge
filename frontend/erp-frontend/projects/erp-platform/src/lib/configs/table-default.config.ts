import { TableConfig } from '../components/generic-table/models/table-config.model';
import { PAGINATION_CONFIG } from './pagination.config';

/**
 * App-level default table configuration. Spread this into a module's table config and override
 * only what differs: `[config]="{ ...TABLE_DEFAULTS, showSelection: true }"`.
 */
export const TABLE_DEFAULTS: TableConfig = {
  showPagination: true,
  showSelection: false,
  multiSelect: false,
  pageSizeOptions: PAGINATION_CONFIG.pageSizeOptions,
  clickableRows: false,
  emptyMessage: 'No records found.',
};
