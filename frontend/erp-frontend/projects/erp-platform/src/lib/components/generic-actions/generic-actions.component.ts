import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PermissionService } from '../../services';
import { ActionConfig, ActionEvent } from './models/action-config.model';

/** How the actions are presented. */
export type ActionDisplayMode = 'buttons' | 'menu';

/**
 * Centralized action component. Renders a set of {@link ActionConfig} as inline buttons or a
 * three-dot menu, honoring permission, hidden and disabled rules, and surfacing confirmation
 * intent so the host can show a dialog before committing.
 *
 * @typeParam T Context passed back with each action (typically the row).
 */
@Component({
  selector: 'app-generic-actions',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatMenuModule, MatTooltipModule],
  templateUrl: './generic-actions.component.html',
  styleUrl: './generic-actions.component.scss',
})
export class GenericActionsComponent<T = unknown> {
  private readonly permissionService = inject(PermissionService);

  private readonly _actions = signal<ActionConfig<T>[]>([]);

  @Input({ required: true })
  set actions(value: ActionConfig<T>[]) {
    this._actions.set(value ?? []);
  }

  /** Context (e.g. the row) passed back with every emitted action. */
  @Input() context!: T;

  @Input() mode: ActionDisplayMode = 'buttons';

  @Input() menuIcon = 'more_vert';

  @Output() actionClick = new EventEmitter<ActionEvent<T>>();

  /** Actions the current user is allowed to see. */
  readonly visibleActions = computed(() =>
    this._actions().filter(
      (action) => this.permissionService.hasPermission(action.permission) && !this.resolve(action.hidden),
    ),
  );

  isDisabled(action: ActionConfig<T>): boolean {
    return this.resolve(action.disabled);
  }

  onTrigger(action: ActionConfig<T>): void {
    if (this.isDisabled(action)) {
      return;
    }
    this.actionClick.emit({ action, context: this.context });
  }

  private resolve(value: boolean | ((context: T) => boolean) | undefined): boolean {
    if (typeof value === 'function') {
      return value(this.context);
    }
    return value ?? false;
  }
}
