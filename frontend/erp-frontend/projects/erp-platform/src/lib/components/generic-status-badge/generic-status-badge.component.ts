import { ChangeDetectionStrategy, Component, Input, computed, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import {
  DEFAULT_STATUS_MAP,
  StatusBadgeColor,
  StatusBadgeConfig,
  StatusBadgeVariant,
} from './models/status-badge.model';

/**
 * Reusable status badge. Accepts either a status string (resolved against {@link DEFAULT_STATUS_MAP},
 * case-insensitive) or a full {@link StatusBadgeConfig}. Color, icon, label and variant are configurable.
 *
 * Usage:
 * ```html
 * <app-generic-status-badge status="Active" />
 * <app-generic-status-badge [status]="{ label: 'On hold', color: 'warning', icon: 'pause' }" variant="filled" />
 * ```
 */
@Component({
  selector: 'app-generic-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule],
  templateUrl: './generic-status-badge.component.html',
  styleUrl: './generic-status-badge.component.scss',
})
export class GenericStatusBadgeComponent {
  private readonly _status = signal<string | StatusBadgeConfig | null>(null);

  /** Status as a known key (e.g. "Active") or a full config object. */
  @Input({ required: true })
  set status(value: string | StatusBadgeConfig | null) {
    this._status.set(value);
  }

  /** Overrides the variant for all statuses. Defaults to 'soft'. */
  @Input() variant: StatusBadgeVariant = 'soft';

  /** Hide the icon even when the resolved config provides one. */
  @Input() showIcon = true;

  /** Resolved, normalized configuration. */
  readonly config = computed<StatusBadgeConfig>(() => {
    const raw = this._status();
    if (raw && typeof raw === 'object') {
      return { color: 'neutral', variant: this.variant, ...raw };
    }
    const key = String(raw ?? '').trim().toLowerCase();
    const mapped = DEFAULT_STATUS_MAP[key];
    return mapped ?? { label: String(raw ?? ''), color: 'neutral' };
  });

  get color(): StatusBadgeColor {
    return this.config().color ?? 'neutral';
  }
}
