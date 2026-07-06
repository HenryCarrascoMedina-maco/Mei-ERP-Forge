import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { KpiStatus, KpiTrend } from './models/kpi-card.model';

/**
 * Reusable KPI / metric card for dashboards. Shows a title, value, optional icon and a
 * trend indicator (up / down / neutral) with a percentage. Supports a loading state and an
 * optional click.
 *
 * Usage:
 * ```html
 * <app-generic-kpi-card title="Active users" [value]="124" icon="group"
 *                       [percentage]="12.5" trend="up" status="success" [clickable]="true"
 *                       (cardClick)="drillDown()" />
 * ```
 */
@Component({
  selector: 'app-generic-kpi-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './generic-kpi-card.component.html',
  styleUrl: './generic-kpi-card.component.scss',
})
export class GenericKpiCardComponent {
  @Input({ required: true }) title!: string;
  @Input() value: string | number | null = null;
  @Input() icon?: string;
  /** Trend percentage (absolute number; sign is implied by `trend`). */
  @Input() percentage?: number;
  @Input() trend: KpiTrend = 'neutral';
  /** Accent color for the icon and value. */
  @Input() status: KpiStatus = 'primary';
  @Input() loading = false;
  @Input() clickable = false;

  @Output() cardClick = new EventEmitter<void>();

  get trendIcon(): string {
    switch (this.trend) {
      case 'up':
        return 'trending_up';
      case 'down':
        return 'trending_down';
      default:
        return 'trending_flat';
    }
  }

  onClick(): void {
    if (this.clickable && !this.loading) {
      this.cardClick.emit();
    }
  }
}
