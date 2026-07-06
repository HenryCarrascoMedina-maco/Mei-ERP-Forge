/**
 * A single breadcrumb node.
 */
export interface BreadcrumbItem {
  label: string;
  /** Router link; when omitted the item renders as plain text (current page). */
  route?: string | unknown[];
  icon?: string;
  disabled?: boolean;
}
