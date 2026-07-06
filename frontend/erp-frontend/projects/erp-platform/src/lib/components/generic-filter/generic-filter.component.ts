import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FilterFieldConfig, FilterValues } from './models/filter-field.model';

/**
 * Reusable, configuration-driven filter bar for tables/lists. Renders a global search plus the
 * configured field filters, and emits the applied {@link FilterValues} (empty/null values stripped)
 * on Apply, or an empty object on Clear. Status filters are just `select`/`boolean` fields.
 *
 * Usage:
 * ```html
 * <app-generic-filter [fields]="filters" (filterChange)="onFilter($event)"></app-generic-filter>
 * ```
 */
@Component({
  selector: 'app-generic-filter',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatButtonModule, MatIconModule,
  ],
  templateUrl: './generic-filter.component.html',
  styleUrl: './generic-filter.component.scss',
})
export class GenericFilterComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  @Input() fields: FilterFieldConfig[] = [];
  @Input() showSearch = true;
  @Input() searchPlaceholder = 'Search…';

  @Output() filterChange = new EventEmitter<FilterValues>();
  @Output() cleared = new EventEmitter<void>();

  form!: FormGroup;

  ngOnInit(): void {
    const group: Record<string, unknown> = {};
    if (this.showSearch) {
      group['search'] = [null];
    }
    for (const field of this.fields) {
      if (field.type === 'daterange') {
        group[`${field.key}From`] = [null];
        group[`${field.key}To`] = [null];
      } else {
        group[field.key] = [null];
      }
    }
    this.form = this.fb.group(group);
  }

  apply(): void {
    this.filterChange.emit(this.clean(this.form.getRawValue()));
  }

  clear(): void {
    this.form.reset();
    this.filterChange.emit({});
    this.cleared.emit();
  }

  private clean(raw: Record<string, unknown>): FilterValues {
    const result: FilterValues = {};
    for (const [key, value] of Object.entries(raw)) {
      if (value === null || value === undefined || value === '') {
        continue;
      }
      result[key] = value instanceof Date ? value.toISOString() : (value as string | number | boolean);
    }
    return result;
  }
}
