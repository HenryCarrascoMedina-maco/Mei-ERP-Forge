import { ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, Output, ViewChild, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FileUtil } from '../../utils/file.util';

/**
 * Reusable import button. Opens a file picker, validates the extension, and emits the selected
 * file. Optionally shows a "download template" action. Basic validation errors are shown inline.
 *
 * Usage:
 * ```html
 * <app-generic-import-button [allowedExtensions]="['csv']" [showTemplate]="true"
 *   (fileSelected)="onImport($event)" (templateDownload)="downloadTemplate()">
 * </app-generic-import-button>
 * ```
 */
@Component({
  selector: 'app-generic-import-button',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatIconModule, MatTooltipModule, MatProgressSpinnerModule],
  templateUrl: './generic-import-button.component.html',
  styleUrl: './generic-import-button.component.scss',
})
export class GenericImportButtonComponent {
  @Input() allowedExtensions: string[] = ['csv'];
  @Input() label = 'Import';
  @Input() icon = 'upload';
  @Input() tooltip?: string;
  @Input() loading = false;
  @Input() disabled = false;
  @Input() showTemplate = false;
  @Input() templateLabel = 'Template';

  /** Emitted when a valid file is chosen (alias of importClick for clarity). */
  @Output() fileSelected = new EventEmitter<File>();
  @Output() importClick = new EventEmitter<File>();
  @Output() templateDownload = new EventEmitter<void>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  readonly error = signal<string | null>(null);

  get accept(): string {
    return this.allowedExtensions.map((e) => `.${e.replace(/^\./, '')}`).join(',');
  }

  openPicker(): void {
    if (!this.disabled && !this.loading) {
      this.fileInput.nativeElement.click();
    }
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    if (this.allowedExtensions.length && !FileUtil.isAllowedExtension(file.name, this.allowedExtensions)) {
      this.error.set(`Unsupported file type. Allowed: ${this.allowedExtensions.join(', ')}.`);
      return;
    }

    this.error.set(null);
    this.fileSelected.emit(file);
    this.importClick.emit(file);
  }
}
