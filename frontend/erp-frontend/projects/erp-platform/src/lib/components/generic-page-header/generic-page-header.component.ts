import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { GenericBreadcrumbComponent } from '../generic-breadcrumb/generic-breadcrumb.component';
import { GenericStatusBadgeComponent } from '../generic-status-badge/generic-status-badge.component';
import { GenericActionsComponent } from '../generic-actions/generic-actions.component';
import { BreadcrumbItem } from '../generic-breadcrumb/models/breadcrumb-item.model';
import { StatusBadgeConfig } from '../generic-status-badge/models/status-badge.model';
import { ActionConfig, ActionEvent } from '../generic-actions/models/action-config.model';
import { PermissionService } from '../../services/permission.service';

/**
 * Standard ERP page header. Composes the breadcrumb, status badge and actions components into a
 * consistent module title bar with an optional primary action, secondary actions and projected
 * content (e.g. inline filters via `[page-header-content]`).
 *
 * Usage:
 * ```html
 * <app-generic-page-header
 *   title="Users" subtitle="Manage system users"
 *   [breadcrumbs]="crumbs" [status]="'Active'"
 *   [primaryAction]="{ key: 'create', label: 'New user', icon: 'add', permission: 'users.create' }"
 *   [secondaryActions]="[{ key: 'export', label: 'Export', icon: 'download' }]"
 *   (actionClick)="onHeaderAction($event)">
 * </app-generic-page-header>
 * ```
 */
@Component({
  selector: 'app-generic-page-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    GenericBreadcrumbComponent,
    GenericStatusBadgeComponent,
    GenericActionsComponent,
  ],
  templateUrl: './generic-page-header.component.html',
  styleUrl: './generic-page-header.component.scss',
})
export class GenericPageHeaderComponent {
  private readonly permissionService = inject(PermissionService);

  @Input({ required: true }) title!: string;
  @Input() subtitle?: string;
  @Input() breadcrumbs: BreadcrumbItem[] = [];
  /** Build the breadcrumb automatically from the route instead of `breadcrumbs`. */
  @Input() autoBreadcrumb = false;
  @Input() status: string | StatusBadgeConfig | null = null;
  @Input() primaryAction: ActionConfig | null = null;
  @Input() secondaryActions: ActionConfig[] = [];

  @Output() actionClick = new EventEmitter<ActionEvent>();

  /** Secondary actions carry no row context; typed as unknown so the actions component infers T=unknown. */
  protected readonly actionContext: unknown = null;

  get showBreadcrumb(): boolean {
    return this.autoBreadcrumb || this.breadcrumbs.length > 0;
  }

  get primaryVisible(): boolean {
    return (
      !!this.primaryAction &&
      this.permissionService.hasPermission(this.primaryAction.permission) &&
      !this.resolve(this.primaryAction.hidden)
    );
  }

  get primaryDisabled(): boolean {
    return this.primaryAction ? this.resolve(this.primaryAction.disabled) : false;
  }

  onPrimary(): void {
    if (this.primaryAction && !this.primaryDisabled) {
      this.actionClick.emit({ action: this.primaryAction, context: null });
    }
  }

  onSecondary(event: ActionEvent): void {
    this.actionClick.emit(event);
  }

  private resolve(value: boolean | ((context: unknown) => boolean) | undefined): boolean {
    return typeof value === 'function' ? value(null) : value ?? false;
  }
}
