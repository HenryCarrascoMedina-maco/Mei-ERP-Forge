import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/permission.service';

/**
 * Protects routes by permission. Reads the required permission(s) from the route's `data`:
 *
 * ```ts
 * {
 *   path: 'users',
 *   canActivate: [permissionGuard],
 *   data: { permission: 'users.view' }                 // single, or
 *   data: { permission: ['users.view','users.manage'], permissionMode: 'any' }
 * }
 * ```
 *
 * Unauthorized access redirects to `/forbidden`.
 */
export const permissionGuard: CanActivateFn = (route) => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);

  const required = route.data['permission'] as string | string[] | undefined;
  if (!required) {
    return true;
  }

  const permissions = Array.isArray(required) ? required : [required];
  const mode = (route.data['permissionMode'] as 'all' | 'any' | undefined) ?? 'all';

  const allowed =
    mode === 'any'
      ? permissionService.hasAnyPermission(permissions)
      : permissions.every((p) => permissionService.hasPermission(p));

  return allowed ? true : router.createUrlTree(['/forbidden']);
};
