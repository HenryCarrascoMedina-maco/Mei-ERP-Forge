import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/permission.service';

/**
 * Protects routes by role. Reads the allowed role(s) from the route's `data.roles`; access is
 * granted when the user holds at least one of them.
 *
 * ```ts
 * { path: 'admin', canActivate: [roleGuard], data: { roles: ['Administrator'] } }
 * ```
 *
 * Unauthorized access redirects to `/forbidden`.
 */
export const roleGuard: CanActivateFn = (route) => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);

  const roles = route.data['roles'] as string | string[] | undefined;
  if (!roles) {
    return true;
  }

  const allowedRoles = Array.isArray(roles) ? roles : [roles];
  const allowed = allowedRoles.some((r) => permissionService.hasRole(r));

  return allowed ? true : router.createUrlTree(['/forbidden']);
};
