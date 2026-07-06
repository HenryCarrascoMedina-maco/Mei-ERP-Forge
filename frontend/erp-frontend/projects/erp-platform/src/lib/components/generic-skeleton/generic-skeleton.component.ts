import { ChangeDetectionStrategy, Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

/** Supported skeleton layouts. */
export type SkeletonType = 'table' | 'form' | 'card' | 'dashboard' | 'detail' | 'list';

/**
 * Reusable loading placeholder. Renders an animated shimmer in one of several shapes so screens
 * keep their layout while data loads. Usable inside `generic-table`, `generic-card` or any page.
 *
 * Usage:
 * ```html
 * <app-generic-skeleton type="table" [rows]="8" [columns]="5" />
 * <app-generic-skeleton type="card" />
 * ```
 */
@Component({
  selector: 'app-generic-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  templateUrl: './generic-skeleton.component.html',
  styleUrl: './generic-skeleton.component.scss',
})
export class GenericSkeletonComponent {
  private readonly _rows = signal(5);
  private readonly _columns = signal(4);

  @Input() type: SkeletonType = 'list';

  @Input()
  set rows(value: number) {
    this._rows.set(Math.max(1, value || 1));
  }

  @Input()
  set columns(value: number) {
    this._columns.set(Math.max(1, value || 1));
  }

  /** Toggle the shimmer animation. */
  @Input() animated = true;

  /** Optional explicit container height/width (any CSS length). */
  @Input() height?: string;
  @Input() width?: string;

  readonly rowItems = computed(() => Array.from({ length: this._rows() }));
  readonly columnItems = computed(() => Array.from({ length: this._columns() }));
}
