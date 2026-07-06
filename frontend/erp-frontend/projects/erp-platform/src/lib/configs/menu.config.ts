/**
 * Sidebar / navigation menu item. Permission-aware and nestable.
 */
export interface MenuItem {
  label: string;
  icon?: string;
  route?: string | unknown[];
  /** Permission required to show the item (filtered via PermissionService). */
  permission?: string;
  /** Optional section divider rendered before this item. */
  divider?: boolean;
  children?: MenuItem[];
}

/**
 * Default navigation menu. Replace/extend per application; the demo only ships a Users entry.
 */
export const MENU_CONFIG: MenuItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/' },
  {
    label: 'Administration',
    icon: 'admin_panel_settings',
    children: [
      { label: 'Users', icon: 'group', route: '/users', permission: 'users.view' },
      { label: 'Roles', icon: 'shield', route: '/roles', permission: 'roles.view' },
    ],
  },
  { label: 'Catalogs', icon: 'list_alt', route: '/catalogs', permission: 'catalogs.view' },
];
