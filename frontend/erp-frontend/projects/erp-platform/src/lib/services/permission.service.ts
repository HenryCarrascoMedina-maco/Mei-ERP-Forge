import { Injectable, signal } from '@angular/core';

/**
 * Holds the current user's permission and role sets and answers authorization checks.
 * Consuming applications populate it after login (e.g. from the JWT or a /me endpoint).
 */
@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly permissions = signal<Set<string>>(new Set());
  private readonly roles = signal<Set<string>>(new Set());

  setPermissions(permissions: string[]): void {
    this.permissions.set(new Set(permissions.map((p) => p.toLowerCase())));
  }

  setRoles(roles: string[]): void {
    this.roles.set(new Set(roles.map((r) => r.toLowerCase())));
  }

  /** True when no permission is required, or the user holds the given permission. */
  hasPermission(permission?: string | null): boolean {
    if (!permission) {
      return true;
    }
    return this.permissions().has(permission.toLowerCase());
  }

  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some((p) => this.hasPermission(p));
  }

  hasRole(role: string): boolean {
    return this.roles().has(role.toLowerCase());
  }

  clear(): void {
    this.permissions.set(new Set());
    this.roles.set(new Set());
  }
}
