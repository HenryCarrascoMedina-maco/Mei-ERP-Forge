import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort as MatSort } from '@angular/material/sort';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { GenericActionsComponent } from '../generic-actions/generic-actions.component';
import { GenericPaginationComponent } from '../generic-pagination/generic-pagination.component';
import { GenericEmptyStateComponent } from '../generic-empty-state/generic-empty-state.component';
import { GenericStatusBadgeComponent } from '../generic-status-badge/generic-status-badge.component';
import { GenericSkeletonComponent } from '../generic-skeleton/generic-skeleton.component';
import { ActionEvent } from '../generic-actions/models/action-config.model';
import { Sort, SortDirection } from '../../models';
import { TableColumn } from './models/table-column.model';
import { TableAction } from './models/table-action.model';
import { DEFAULT_TABLE_CONFIG, TableConfig } from './models/table-config.model';
import { PageChangeEvent } from './models/table-event.model';

type Row = Record<string, unknown>;

/**
 * Reusable, configuration-driven data table for ERP modules. The host owns the data and
 * reacts to {@link pageChange} / {@link sortChange} (server-side paging by default); the
 * table renders columns, row selection, row actions, loading and empty states.
 *
 * @typeParam T Row data type.
 */
@Component({
  selector: 'app-generic-table',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatCheckboxModule,
    MatProgressBarModule,
    GenericActionsComponent,
    GenericPaginationComponent,
    GenericEmptyStateComponent,
    GenericStatusBadgeComponent,
    GenericSkeletonComponent,
  ],
  templateUrl: './generic-table.component.html',
  styleUrl: './generic-table.component.scss',
})
export class GenericTableComponent<T extends Row = Row> {
  private readonly _columns = signal<TableColumn<T>[]>([]);
  private readonly _data = signal<T[]>([]);
  private readonly _config = signal<TableConfig>({});

  @Input({ required: true })
  set columns(value: TableColumn<T>[]) {
    this._columns.set(value ?? []);
  }

  @Input({ required: true })
  set data(value: T[]) {
    this._data.set(value ?? []);
    this.selected.set(new Set());
  }

  @Input()
  set config(value: TableConfig) {
    this._config.set(value ?? {});
  }

  /** Row-level actions rendered in the trailing actions column. */
  @Input() rowActions: TableAction<T>[] = [];

  /** Total record count for pagination (server-side). Defaults to the local data length. */
  @Input() totalItems = 0;
  @Input() page = 1;
  @Input() pageSize = 10;
  @Input() loading = false;

  @Output() pageChange = new EventEmitter<PageChangeEvent>();
  @Output() sortChange = new EventEmitter<Sort>();
  @Output() actionClick = new EventEmitter<ActionEvent<T>>();
  @Output() rowClick = new EventEmitter<T>();
  @Output() selectionChange = new EventEmitter<T[]>();

  /** Set of currently selected rows. */
  private readonly selected = signal<Set<T>>(new Set());

  readonly rows = computed(() => this._data());

  readonly resolvedConfig = computed<TableConfig>(() => ({ ...DEFAULT_TABLE_CONFIG, ...this._config() }));

  readonly visibleColumns = computed(() => this._columns().filter((c) => c.visible !== false));

  readonly displayedColumns = computed(() => {
    const cols: string[] = [];
    if (this.resolvedConfig().showSelection) {
      cols.push('__select');
    }
    cols.push(...this.visibleColumns().map((c) => c.key));
    if (this.rowActions.length > 0) {
      cols.push('__actions');
    }
    return cols;
  });

  readonly total = computed(() => this.totalItems || this._data().length);

  /** First-load state: show a skeleton instead of an empty grid while data is fetched. */
  get showSkeleton(): boolean {
    return this.loading && this._data().length === 0;
  }

  // --- Cell rendering -------------------------------------------------------

  formatCell(column: TableColumn<T>, row: T): string {
    const value = row[column.key];
    if (column.formatter) {
      return column.formatter(value, row);
    }
    if (value === null || value === undefined) {
      return '';
    }
    switch (column.type) {
      case 'boolean':
        return value ? 'Yes' : 'No';
      case 'date':
        return new Date(value as string).toLocaleDateString();
      case 'datetime':
        return new Date(value as string).toLocaleString();
      default:
        return String(value);
    }
  }

  // --- Sorting --------------------------------------------------------------

  onSort(sort: MatSort): void {
    if (!sort.active || sort.direction === '') {
      return;
    }
    const direction: SortDirection = sort.direction === 'desc' ? 'Descending' : 'Ascending';
    this.sortChange.emit({ sortBy: sort.active, sortDirection: direction });
  }

  // --- Selection ------------------------------------------------------------

  isSelected(row: T): boolean {
    return this.selected().has(row);
  }

  toggleRow(row: T): void {
    const next = new Set(this.selected());
    if (next.has(row)) {
      next.delete(row);
    } else {
      if (!this.resolvedConfig().multiSelect) {
        next.clear();
      }
      next.add(row);
    }
    this.selected.set(next);
    this.selectionChange.emit([...next]);
  }

  get allSelected(): boolean {
    const rows = this._data();
    return rows.length > 0 && rows.every((r) => this.selected().has(r));
  }

  get someSelected(): boolean {
    return this.selected().size > 0 && !this.allSelected;
  }

  toggleAll(): void {
    const next = this.allSelected ? new Set<T>() : new Set<T>(this._data());
    this.selected.set(next);
    this.selectionChange.emit([...next]);
  }

  // --- Events ---------------------------------------------------------------

  onRowClick(row: T): void {
    if (this.resolvedConfig().clickableRows) {
      this.rowClick.emit(row);
    }
  }

  onPage(event: PageChangeEvent): void {
    this.pageChange.emit(event);
  }

  onAction(event: ActionEvent<T>): void {
    this.actionClick.emit(event);
  }
}
