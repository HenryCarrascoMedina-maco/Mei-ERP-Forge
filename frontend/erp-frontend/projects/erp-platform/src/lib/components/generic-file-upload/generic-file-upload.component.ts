import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { FileSizePipe } from '../../pipes/file-size.pipe';
import { FileUtil } from '../../utils/file.util';

/**
 * Reusable file selector with drag & drop, extension/size/count validation and a preview list.
 * Selection only — uploading is left to the host (see `FileService`). Emits the selected files
 * and any validation errors.
 *
 * Usage:
 * ```html
 * <app-generic-file-upload
 *   [allowedExtensions]="['png','jpg','pdf']" [maxSizeMb]="5" [maxFiles]="3" [multiple]="true"
 *   (filesSelected)="onFiles($event)" (errors)="onErrors($event)">
 * </app-generic-file-upload>
 * ```
 */
@Component({
  selector: 'app-generic-file-upload',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatIconModule, MatButtonModule, FileSizePipe],
  templateUrl: './generic-file-upload.component.html',
  styleUrl: './generic-file-upload.component.scss',
})
export class GenericFileUploadComponent {
  @Input() allowedExtensions: string[] = [];
  @Input() maxSizeMb?: number;
  @Input() maxFiles = 1;
  @Input() multiple = false;
  @Input() label = 'Drag & drop files here, or click to browse';
  @Input() disabled = false;

  @Output() filesSelected = new EventEmitter<File[]>();
  @Output() errors = new EventEmitter<string[]>();

  readonly files = signal<File[]>([]);
  readonly validationErrors = signal<string[]>([]);
  readonly dragOver = signal(false);

  get accept(): string {
    return this.allowedExtensions.map((e) => `.${e.replace(/^\./, '')}`).join(',');
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.disabled) {
      this.dragOver.set(true);
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
    if (this.disabled || !event.dataTransfer) {
      return;
    }
    this.handleFiles(Array.from(event.dataTransfer.files));
  }

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
    }
    input.value = '';
  }

  remove(index: number): void {
    const next = this.files().filter((_, i) => i !== index);
    this.files.set(next);
    this.filesSelected.emit(next);
  }

  private handleFiles(incoming: File[]): void {
    const combined = this.multiple ? [...this.files(), ...incoming] : incoming.slice(0, 1);
    const errors: string[] = [];
    const valid: File[] = [];

    for (const file of combined) {
      if (this.allowedExtensions.length && !FileUtil.isAllowedExtension(file.name, this.allowedExtensions)) {
        errors.push(`"${file.name}" has an unsupported type.`);
        continue;
      }
      if (this.maxSizeMb !== undefined && !FileUtil.isWithinSize(file.size, this.maxSizeMb)) {
        errors.push(`"${file.name}" exceeds the ${this.maxSizeMb} MB limit.`);
        continue;
      }
      valid.push(file);
    }

    let accepted = valid;
    if (valid.length > this.maxFiles) {
      errors.push(`A maximum of ${this.maxFiles} file(s) is allowed.`);
      accepted = valid.slice(0, this.maxFiles);
    }

    this.files.set(accepted);
    this.validationErrors.set(errors);
    this.filesSelected.emit(accepted);
    this.errors.emit(errors);
  }
}
