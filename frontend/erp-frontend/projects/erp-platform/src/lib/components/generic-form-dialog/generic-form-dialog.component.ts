import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { GenericModalComponent } from '../generic-modal/generic-modal.component';
import { GenericFormComponent } from '../generic-form/generic-form.component';
import { FormFieldConfig } from '../generic-form/models/form-field-config.model';
import { FormMode } from '../generic-form/models/form-section.model';

/** Data contract for the generic form dialog. */
export interface FormDialogData<T = Record<string, unknown>> {
  title: string;
  fields: FormFieldConfig<T>[];
  mode?: FormMode;
  value?: Partial<T> | null;
  submitLabel?: string;
}

/**
 * Reusable form dialog: composes {@link GenericModalComponent} + {@link GenericFormComponent} so
 * feature modules get create/edit/view dialogs from a field config alone — no per-module dialog
 * components. Opened via {@link DialogService.openForm}; resolves to the submitted value (or undefined).
 */
@Component({
  selector: 'app-generic-form-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GenericModalComponent, GenericFormComponent],
  template: `
    <app-generic-modal [title]="data.title" [showFooter]="false" (closed)="close()">
      <app-generic-form
        [fields]="data.fields"
        [mode]="data.mode ?? 'create'"
        [value]="data.value ?? null"
        [submitLabel]="data.submitLabel ?? 'Save'"
        (formSubmit)="submit($event)"
        (cancel)="close()">
      </app-generic-form>
    </app-generic-modal>
  `,
})
export class GenericFormDialogComponent {
  constructor(
    private readonly dialogRef: MatDialogRef<GenericFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public readonly data: FormDialogData,
  ) {}

  submit(value: Record<string, unknown>): void {
    this.dialogRef.close(value);
  }

  close(): void {
    this.dialogRef.close();
  }
}
