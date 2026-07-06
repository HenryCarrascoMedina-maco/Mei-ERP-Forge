/**
 * Maps route paths to the permission(s) required to access them. The `permissionGuard` reads
 * permissions from route `data`, but this central map is useful for menu filtering and tests.
 *
 * Convention: key = route path, value = required permission or list (any-of).
 */
export type RoutePermissionMap = Record<string, string | string[]>;

export const ROUTE_PERMISSIONS: RoutePermissionMap = {
  '/users': 'users.view',
  '/users/new': 'users.create',
  '/roles': 'roles.view',
  '/catalogs': 'catalogs.view',
};
