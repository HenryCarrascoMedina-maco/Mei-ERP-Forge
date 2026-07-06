import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';

import { FormFieldConfig } from './models/form-field-config.model';
import { FormMode, FormSection } from './models/form-section.model';
import { FieldChangeEvent } from './models/form-event.model';

type FormValue = Record<string, unknown>;

/**
 * Configuration-driven dynamic form. Builds a reactive form from {@link FormSection} /
 * {@link FormFieldConfig} definitions and supports create / edit / view modes, conditional
 * visibility and conditional enable/disable.
 *
 * @typeParam T The form model type.
 */
@Component({
  selector: 'app-generic-form',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatSlideToggleModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
  ],
  templateUrl: './generic-form.component.html',
  styleUrl: './generic-form.component.scss',
})
export class GenericFormComponent<T extends FormValue = FormValue> implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  /** Sections to render. For a flat form, use a single untitled section. */
  @Input() sections: FormSection<T>[] = [];

  /** Convenience input: a flat field list, wrapped into a single section. */
  @Input() fields: FormFieldConfig<T>[] = [];

  @Input() mode: FormMode = 'create';

  /** Initial / current model value (used in edit and view modes). */
  @Input() value: Partial<T> | null = null;

  @Input() submitLabel = 'Save';
  @Input() cancelLabel = 'Cancel';
  @Input() showActions = true;

  @Output() formSubmit = new EventEmitter<T>();
  @Output() cancel = new EventEmitter<void>();
  @Output() valueChange = new EventEmitter<Partial<T>>();
  @Output() fieldChange = new EventEmitter<FieldChangeEvent>();

  form: FormGroup = this.fb.group({});

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['sections'] || changes['fields'] || changes['mode']) {
      this.buildForm();
    }
    if (changes['value'] && this.value) {
      this.form.patchValue(this.value, { emitEvent: false });
    }
  }

  /** Effective section list, normalizing the flat `fields` input. */
  get effectiveSections(): FormSection<T>[] {
    if (this.sections.length > 0) {
      return this.sections;
    }
    return this.fields.length > 0 ? [{ fields: this.fields }] : [];
  }

  get isView(): boolean {
    return this.mode === 'view';
  }

  private get allFields(): FormFieldConfig<T>[] {
    return this.effectiveSections.flatMap((s) => s.fields);
  }

  private buildForm(): void {
    const group: Record<string, unknown> = {};
    for (const field of this.allFields) {
      const initial = this.value?.[field.key] ?? field.defaultValue ?? this.emptyFor(field);
      group[field.key] = [{ value: initial, disabled: this.isView }, field.validators ?? []];
    }
    this.form = this.fb.group(group);

    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((v) => {
      this.applyConditionalState(v as Partial<T>);
      this.valueChange.emit(v as Partial<T>);
    });

    this.applyConditionalState(this.form.getRawValue() as Partial<T>);
  }

  /** Re-evaluates conditional disabled state for each control. */
  private applyConditionalState(value: Partial<T>): void {
    if (this.isView) {
      return;
    }
    for (const field of this.allFields) {
      const control = this.form.get(field.key);
      if (!control) {
        continue;
      }
      const shouldDisable = this.resolve(field.disabled, value);
      if (shouldDisable && control.enabled) {
        control.disable({ emitEvent: false });
      } else if (!shouldDisable && control.disabled) {
        control.enable({ emitEvent: false });
      }
    }
  }

  isHidden(field: FormFieldConfig<T>): boolean {
    return this.resolve(field.hidden, this.form.getRawValue() as Partial<T>);
  }

  onFieldInput(field: FormFieldConfig<T>): void {
    this.fieldChange.emit({ key: field.key, value: this.form.get(field.key)?.value });
  }

  onSubmit(): void {
    if (this.isView) {
      return;
    }
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }
    this.formSubmit.emit(this.form.getRawValue() as T);
  }

  reset(): void {
    this.form.reset();
    if (this.value) {
      this.form.patchValue(this.value, { emitEvent: false });
    }
  }

  private resolve(
    value: boolean | ((value: Partial<T>) => boolean) | undefined,
    formValue: Partial<T>,
  ): boolean {
    if (typeof value === 'function') {
      return value(formValue);
    }
    return value ?? false;
  }

  private emptyFor(field: FormFieldConfig<T>): unknown {
    if (field.type === 'checkbox' || field.type === 'switch') {
      return false;
    }
    if (field.type === 'multiselect') {
      return [];
    }
    return null;
  }
}
