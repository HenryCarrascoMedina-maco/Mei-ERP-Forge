/**
 * A single tab definition for the generic tabs component.
 */
export interface TabItem {
  /** Stable key emitted on selection and used to match projected content. */
  key: string;
  label: string;
  icon?: string;
  disabled?: boolean;
  /** Permission required to see the tab (checked via PermissionService). */
  permission?: string;
  /** Router link for route-driven tabs (used when the component is in 'route' mode). */
  route?: string | unknown[];
}
