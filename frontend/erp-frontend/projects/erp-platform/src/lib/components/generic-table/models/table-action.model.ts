import { ActionConfig } from '../../generic-actions/models/action-config.model';

/**
 * Row/header action for the table. Reuses the shared {@link ActionConfig} contract so the
 * table and the standalone actions component stay aligned.
 *
 * @typeParam T Row data type.
 */
export type TableAction<T = Record<string, unknown>> = ActionConfig<T>;
