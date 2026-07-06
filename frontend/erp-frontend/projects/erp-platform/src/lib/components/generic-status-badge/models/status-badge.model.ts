/** Semantic color tokens for a status badge (mapped to SCSS classes). */
export type StatusBadgeColor = 'success' | 'warning' | 'error' | 'info' | 'neutral' | 'primary';

/** Visual style of the badge. */
export type StatusBadgeVariant = 'soft' | 'filled' | 'outlined';

/** Full badge configuration. */
export interface StatusBadgeConfig {
  label: string;
  color?: StatusBadgeColor;
  icon?: string;
  variant?: StatusBadgeVariant;
}

/**
 * Default mapping for common ERP statuses (case-insensitive lookup by key). Consuming apps can
 * pass a full {@link StatusBadgeConfig} to override or extend these.
 */
export const DEFAULT_STATUS_MAP: Record<string, StatusBadgeConfig> = {
  active: { label: 'Active', color: 'success', icon: 'check_circle' },
  inactive: { label: 'Inactive', color: 'neutral', icon: 'cancel' },
  pending: { label: 'Pending', color: 'warning', icon: 'schedule' },
  approved: { label: 'Approved', color: 'success', icon: 'verified' },
  rejected: { label: 'Rejected', color: 'error', icon: 'block' },
  completed: { label: 'Completed', color: 'success', icon: 'task_alt' },
  error: { label: 'Error', color: 'error', icon: 'error' },
  draft: { label: 'Draft', color: 'neutral', icon: 'edit_note' },
  archived: { label: 'Archived', color: 'info', icon: 'inventory_2' },
};
