import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { GenericActionsComponent } from '../generic-actions/generic-actions.component';
import { ActionConfig, ActionEvent } from '../generic-actions/models/action-config.model';

/**
 * Reusable card container for sections, summaries and dashboards. Provides a header
 * (icon + title + subtitle + optional actions), a loading bar, projected content and an
 * optional projected footer.
 *
 * Usage:
 * ```html
 * <app-generic-card title="Users" subtitle="Active directory" icon="group" [actions]="cardActions"
 *                   [loading]="loading()" (actionClick)="onCardAction($event)">
 *   <p>Body content…</p>
 *   <div card-footer>Footer content</div>
 * </app-generic-card>
 * ```
 */
@Component({
  selector: 'app-generic-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatCardModule, MatIconModule, MatProgressBarModule, GenericActionsComponent],
  templateUrl: './generic-card.component.html',
  styleUrl: './generic-card.component.scss',
})
export class GenericCardComponent {
  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() loading = false;
  /** Header actions, rendered with the shared actions component. */
  @Input() actions: ActionConfig[] = [];
  /** Display the header actions as inline buttons or a three-dot menu. */
  @Input() actionsMode: 'buttons' | 'menu' = 'buttons';
  /** Set false to render without the Material elevation/border. */
  @Input() outlined = false;

  @Output() actionClick = new EventEmitter<ActionEvent>();

  /** Header actions carry no row context; typed as unknown so the actions component infers T=unknown. */
  protected readonly actionContext: unknown = null;

  get hasHeader(): boolean {
    return !!(this.title || this.subtitle || this.icon || this.actions.length);
  }
}
