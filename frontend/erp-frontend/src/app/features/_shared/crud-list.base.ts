import { inject, signal } from '@angular/core';
import { BaseCrudService } from '@erp-platform/core';
import { DialogService } from '@erp-platform/core';
import { ToastService } from '@erp-platform/core';
import { PaginationRequest, Sort } from '@erp-platform/core';
import { PageChangeEvent } from '@erp-platform/core';
import { FormFieldConfig } from '@erp-platform/core';
import { FilterValues } from '@erp-platform/core';

/**
 * Reusable CRUD list controller for feature modules. Encapsulates load / paginate / sort / filter /
 * create / edit / delete against a {@link BaseCrudService}, using only shared services and the
 * reusable form/confirm dialogs. Modules extend this and supply configuration (service, columns,
 * form fields) — no per-module CRUD logic is duplicated.
 */
export abstract class CrudListBase<TDto extends Record<string, unknown> & { id: string }> {
  protected readonly dialog = inject(DialogService);
  protected readonly toast = inject(ToastService);

  /** The module's CRUD service (extends BaseCrudService). */
  protected abstract readonly service: BaseCrudService<TDto, Partial<TDto>, Partial<TDto>, string>;
  /** Field configuration for the create/edit dialog. */
  protected abstract readonly formFields: FormFieldConfig<TDto>[];
  /** Human-readable entity name used in messages/titles, e.g. "Category". */
  protected abstract readonly entityName: string;

  readonly rows = signal<TDto[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);

  page = 1;
  pageSize = 10;
  protected sort?: Sort;
  protected filters: FilterValues = {};

  /** Label shown in the delete confirmation; override for a friendlier field. */
  protected labelOf(row: TDto): string {
    return (row['name'] ?? row['fullName'] ?? row['code'] ?? row.id) as string;
  }

  load(): void {
    this.loading.set(true);
    const { search, ...rest } = this.filters;
    const request: PaginationRequest = {
      page: this.page,
      pageSize: this.pageSize,
      search: (search as string) ?? null,
      sortBy: this.sort?.sortBy,
      sortDirection: this.sort?.sortDirection,
    };

    this.service.list(request, rest).subscribe({
      next: (res) => {
        this.rows.set(res.data);
        this.total.set(res.meta.totalItems);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPage(event: PageChangeEvent): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }

  onSort(sort: Sort): void {
    this.sort = sort;
    this.load();
  }

  onFilter(values: FilterValues): void {
    this.filters = values;
    this.page = 1;
    this.load();
  }

  openCreate(): void {
    this.dialog
      .openForm<TDto>({ title: `New ${this.entityName}`, fields: this.formFields, mode: 'create' })
      .subscribe((value) => {
        if (!value) return;
        this.service.create(value).subscribe(() => {
          this.toast.success(`${this.entityName} created successfully.`);
          this.load();
        });
      });
  }

  openEdit(row: TDto): void {
    this.dialog
      .openForm<TDto>({ title: `Edit ${this.entityName}`, fields: this.formFields, mode: 'edit', value: row })
      .subscribe((value) => {
        if (!value) return;
        this.service.update(row.id, value).subscribe(() => {
          this.toast.success(`${this.entityName} updated successfully.`);
          this.load();
        });
      });
  }

  openView(row: TDto): void {
    this.dialog.openForm<TDto>({
      title: `${this.entityName} details`,
      fields: this.formFields,
      mode: 'view',
      value: row,
    });
  }

  confirmDelete(row: TDto): void {
    this.dialog
      .confirm({
        title: `Delete ${this.entityName}`,
        message: `Are you sure you want to delete "${this.labelOf(row)}"? This action cannot be undone.`,
        confirmText: 'Delete',
        color: 'warn',
        icon: 'warning',
      })
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe(() => {
          this.toast.success(`${this.entityName} deleted successfully.`);
          this.load();
        });
      });
  }
}
