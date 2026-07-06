import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

/** Data contract for the confirmation dialog. */
export interface ConfirmDialogData {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  /** Emphasis of the confirm button; use 'warn' for destructive actions. */
  color?: 'primary' | 'accent' | 'warn';
  icon?: string;
}

/**
 * Reusable confirmation dialog for critical actions (delete, deactivate, approve, ...).
 * Opened via {@link DialogService.confirm}; resolves to `true` when confirmed.
 */
@Component({
  selector: 'app-generic-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './generic-confirm-dialog.component.html',
  styleUrl: './generic-confirm-dialog.component.scss',
})
export class GenericConfirmDialogComponent {
  constructor(
    private readonly dialogRef: MatDialogRef<GenericConfirmDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) public readonly data: ConfirmDialogData,
  ) {}

  get color(): 'primary' | 'accent' | 'warn' {
    return this.data.color ?? 'primary';
  }

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
