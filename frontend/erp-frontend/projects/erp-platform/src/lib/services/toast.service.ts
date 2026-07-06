import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

/**
 * Thin wrapper over Angular Material's snackbar for consistent success / error /
 * warning / info notifications across the app.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly snackBar = inject(MatSnackBar);

  private readonly defaults: MatSnackBarConfig = {
    duration: 4000,
    horizontalPosition: 'right',
    verticalPosition: 'top',
  };

  success(message: string, action = 'OK'): void {
    this.show(message, action, 'success');
  }

  error(message: string, action = 'OK'): void {
    this.show(message, action, 'error', 6000);
  }

  warning(message: string, action = 'OK'): void {
    this.show(message, action, 'warning');
  }

  info(message: string, action = 'OK'): void {
    this.show(message, action, 'info');
  }

  private show(message: string, action: string, type: ToastType, duration?: number): void {
    this.snackBar.open(message, action, {
      ...this.defaults,
      duration: duration ?? this.defaults.duration,
      panelClass: [`toast-${type}`],
    });
  }
}
