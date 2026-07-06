import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Group-level validator ensuring a start date is not after an end date. Apply it to the
 * FormGroup that contains both controls. Empty bounds pass. Error key (on the group): `dateRange`.
 *
 * Usage:
 * ```ts
 * this.fb.group({ from: [null], to: [null] }, { validators: dateRangeValidator('from', 'to') })
 * ```
 */
export function dateRangeValidator(startKey: string, endKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get(startKey)?.value;
    const end = group.get(endKey)?.value;

    if (!start || !end) {
      return null;
    }

    const startTime = new Date(start).getTime();
    const endTime = new Date(end).getTime();

    if (Number.isNaN(startTime) || Number.isNaN(endTime)) {
      return null;
    }

    return startTime > endTime ? { dateRange: { startKey, endKey } } : null;
  };
}
