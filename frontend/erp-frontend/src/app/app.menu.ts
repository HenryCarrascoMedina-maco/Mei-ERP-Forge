import { MenuItem } from '@erp-platform/core';

/**
 * Application navigation menu. Generated module entries are inserted at the anchor below by the
 * `@erp-platform:module` schematic (idempotent). Hand-edit the static entries freely.
 */
export const APP_MENU: MenuItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
  { label: 'Customers', icon: 'groups', route: '/customers' },
  { label: 'Products', icon: 'inventory_2', route: '/products' },
  { label: 'Users', icon: 'group', route: '/security/users' },
  { label: 'Roles', icon: 'admin_panel_settings', route: '/security/roles' },
  // erp-generated:menu
];
