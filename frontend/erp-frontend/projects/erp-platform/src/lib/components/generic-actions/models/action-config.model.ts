/** Visual emphasis for an action. */
export type ActionColor = 'primary' | 'accent' | 'warn' | 'default';

/**
 * Defines a single action (row action, header action or toolbar button).
 *
 * @typeParam T Context data passed to predicates (e.g. the row).
 */
export interface ActionConfig<T = unknown> {
  /** Unique key emitted when the action is triggered. */
  key: string;
  label: string;
  icon?: string;
  color?: ActionColor;
  /** Permission required to see/use the action. Checked by the permission service. */
  permission?: string;
  /** Hide the action, optionally based on the context row. */
  hidden?: boolean | ((context: T) => boolean);
  /** Disable the action, optionally based on the context row. */
  disabled?: boolean | ((context: T) => boolean);
  /** When true, a confirmation dialog is shown before the action is emitted. */
  requiresConfirmation?: boolean;
  /** Optional confirmation message override. */
  confirmationMessage?: string;
  tooltip?: string;
}

/** Event payload emitted when an action is triggered. */
export interface ActionEvent<T = unknown> {
  action: ActionConfig<T>;
  context: T;
}
