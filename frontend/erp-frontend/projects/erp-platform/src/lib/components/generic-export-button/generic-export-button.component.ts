import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ExportType } from '../../services/export.service';

const TYPE_META: Record<ExportType, { label: string; icon: string }> = {
  csv: { label: 'CSV', icon: 'description' },
  excel: { label: 'Excel', icon: 'grid_on' },
  pdf: { label: 'PDF', icon: 'picture_as_pdf' },
};

/**
 * Reusable export button. With a single type it renders a button; with several it renders a
 * menu. Emits {@link exportClick} with the chosen {@link ExportType}; the host performs the export.
 *
 * Usage:
 * ```html
 * <app-generic-export-button [types]="['csv','excel']" [loading]="exporting()"
 *                            (exportClick)="onExport($event)"></app-generic-export-button>
 * ```
 */
@Component({
  selector: 'app-generic-export-button',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatIconModule, MatMenuModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './generic-export-button.component.html',
})
export class GenericExportButtonComponent {
  @Input() types: ExportType[] = ['csv'];
  @Input() label = 'Export';
  @Input() icon = 'download';
  @Input() tooltip?: string;
  @Input() loading = false;
  @Input() disabled = false;

  @Output() exportClick = new EventEmitter<ExportType>();

  get isMenu(): boolean {
    return this.types.length > 1;
  }

  meta(type: ExportType): { label: string; icon: string } {
    return TYPE_META[type];
  }

  trigger(type: ExportType): void {
    if (!this.disabled && !this.loading) {
      this.exportClick.emit(type);
    }
  }
}
