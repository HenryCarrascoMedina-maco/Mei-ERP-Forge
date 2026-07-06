import { Directive, Input, TemplateRef, ViewContainerRef, effect, inject } from '@angular/core';
import { PermissionService } from '../services/permission.service';

/**
 * Structural directive that renders its host element only when the current user holds the
 * required permission(s). Reacts to permission changes via the {@link PermissionService} signals.
 *
 * Usage:
 * ```html
 * <button *appHasPermission="'users.create'">New</button>
 * <button *appHasPermission="['users.update','users.delete']; mode: 'any'">Manage</button>
 * ```
 */
@Directive({
  selector: '[appHasPermission]',
  standalone: true,
})
export class HasPermissionDirective {
  private readonly permissionService = inject(PermissionService);
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);

  private permissions: string[] = [];
  /** 'all' requires every permission; 'any' requires at least one. */
  private mode: 'all' | 'any' = 'all';
  private rendered = false;

  constructor() {
    // Re-evaluate whenever the directive inputs or the permission set change.
    effect(() => this.updateView());
  }

  @Input()
  set appHasPermission(value: string | string[]) {
    this.permissions = Array.isArray(value) ? value : [value];
    this.updateView();
  }

  @Input()
  set appHasPermissionMode(value: 'all' | 'any') {
    this.mode = value;
    this.updateView();
  }

  private updateView(): void {
    const allowed =
      this.permissions.length === 0
        ? true
        : this.mode === 'any'
          ? this.permissionService.hasAnyPermission(this.permissions)
          : this.permissions.every((p) => this.permissionService.hasPermission(p));

    if (allowed && !this.rendered) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.rendered = true;
    } else if (!allowed && this.rendered) {
      this.viewContainer.clear();
      this.rendered = false;
    }
  }
}
