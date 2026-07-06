import { Injectable, inject } from '@angular/core';
import { ComponentType } from '@angular/cdk/portal';
import { MatDialog, MatDialogConfig, MatDialogRef } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';
import {
  ConfirmDialogData,
  GenericConfirmDialogComponent,
} from '../components/generic-confirm-dialog/generic-confirm-dialog.component';
import {
  FormDialogData,
  GenericFormDialogComponent,
} from '../components/generic-form-dialog/generic-form-dialog.component';

/**
 * Centralizes opening of modals and confirmation dialogs so feature code never touches
 * {@link MatDialog} configuration directly.
 */
@Injectable({ providedIn: 'root' })
export class DialogService {
  private readonly dialog = inject(MatDialog);

  /** Opens the standard confirmation dialog and resolves to true when confirmed. */
  confirm(data: ConfirmDialogData): Observable<boolean> {
    return this.dialog
      .open<GenericConfirmDialogComponent, ConfirmDialogData, boolean>(GenericConfirmDialogComponent, {
        width: '420px',
        autoFocus: false,
        data,
      })
      .afterClosed()
      .pipe(map((result) => result === true));
  }

  /**
   * Opens the reusable form dialog and resolves to the submitted value, or undefined when
   * cancelled/closed.
   */
  openForm<T extends Record<string, unknown>>(data: FormDialogData<T>): Observable<T | undefined> {
    return this.dialog
      .open<GenericFormDialogComponent, FormDialogData, T>(GenericFormDialogComponent, {
        width: '640px',
        autoFocus: false,
        data: data as FormDialogData,
      })
      .afterClosed();
  }

  /** Opens an arbitrary component as a modal. */
  open<TComponent, TData = unknown, TResult = unknown>(
    component: ComponentType<TComponent>,
    config?: MatDialogConfig<TData>,
  ): MatDialogRef<TComponent, TResult> {
    return this.dialog.open<TComponent, TData, TResult>(component, {
      width: '600px',
      autoFocus: false,
      ...config,
    });
  }
}
