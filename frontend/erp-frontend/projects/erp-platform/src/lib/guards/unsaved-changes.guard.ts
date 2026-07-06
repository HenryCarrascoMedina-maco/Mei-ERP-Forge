import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { Observable, of } from 'rxjs';
import { DialogService } from '../services/dialog.service';

/**
 * Contract implemented by components that want to block navigation while they hold unsaved
 * changes. Return `true` to allow leaving without prompting.
 */
export interface CanComponentDeactivate {
  canDeactivate(): boolean | Observable<boolean>;
}

/**
 * Prevents leaving a route with unsaved changes. When the component reports it cannot
 * deactivate, a confirmation dialog asks the user to discard their changes.
 *
 * Usage: `{ path: 'users/:id', component: UserEditComponent, canDeactivate: [unsavedChangesGuard] }`
 * where `UserEditComponent implements CanComponentDeactivate`.
 */
export const unsavedChangesGuard: CanDeactivateFn<CanComponentDeactivate> = (component) => {
  if (!component?.canDeactivate) {
    return true;
  }

  const result = component.canDeactivate();
  const canLeave$ = typeof result === 'boolean' ? of(result) : result;

  const dialog = inject(DialogService);

  return new Observable<boolean>((subscriber) => {
    canLeave$.subscribe((canLeave) => {
      if (canLeave) {
        subscriber.next(true);
        subscriber.complete();
        return;
      }

      dialog
        .confirm({
          title: 'Discard changes?',
          message: 'You have unsaved changes. Are you sure you want to leave this page?',
          confirmText: 'Discard',
          cancelText: 'Stay',
          color: 'warn',
          icon: 'warning',
        })
        .subscribe((confirmed) => {
          subscriber.next(confirmed);
          subscriber.complete();
        });
    });
  });
};
