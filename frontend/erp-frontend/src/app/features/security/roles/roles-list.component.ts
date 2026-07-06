import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { Validators } from '@angular/forms';
import {
  GenericPageHeaderComponent, GenericCardComponent, GenericTableComponent, GenericFilterComponent,
  TableColumn, TableAction, TableConfig, ActionConfig, ActionEvent, FilterFieldConfig,
  FormFieldConfig, BreadcrumbItem, SelectOption, TABLE_DEFAULTS,
} from '@erp-platform/core';
import { CrudListBase } from '../../_shared/crud-list.base';
import { RolesService } from './roles.service';
import { SecurityRole } from './roles.model';

const ROLE_PERMISSIONS = { view: 'roles.view', create: 'roles.create', update: 'roles.update', delete: 'roles.delete' };

/** Role management screen — composed entirely from shared components + CrudListBase. */
@Component({
  selector: 'app-roles-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GenericPageHeaderComponent, GenericCardComponent, GenericTableComponent, GenericFilterComponent],
  template: `
    <div class="page">
      <app-generic-page-header
        title="Roles"
        [breadcrumbs]="breadcrumbs"
        [primaryAction]="newAction"
        (actionClick)="openCreate()"></app-generic-page-header>

      <app-generic-card>
        <app-generic-filter [fields]="filterFields" (filterChange)="onFilter($event)" />
      </app-generic-card>

      <app-generic-card>
        <app-generic-table
          [columns]="columns"
          [data]="rows()"
          [config]="config"
          [rowActions]="rowActions"
          [loading]="loading()"
          [page]="page"
          [pageSize]="pageSize"
          [totalItems]="total()"
          (pageChange)="onPage($event)"
          (sortChange)="onSort($event)"
          (actionClick)="onAction($event)"></app-generic-table>
      </app-generic-card>
    </div>
  `,
})
export class RolesListComponent extends CrudListBase<SecurityRole> implements OnInit {
  protected readonly service = inject(RolesService);
  protected readonly entityName = 'Role';

  private readonly permissionOptions = signal<SelectOption[]>([]);

  readonly breadcrumbs: BreadcrumbItem[] = [
    { label: 'Home', route: '/', icon: 'home' }, { label: 'Security' }, { label: 'Roles' },
  ];
  readonly newAction: ActionConfig = { key: 'create', label: 'New role', icon: 'add', permission: ROLE_PERMISSIONS.create };

  readonly columns: TableColumn<SecurityRole>[] = [
    { key: 'name', label: 'Name', sortable: true, sticky: true },
    { key: 'description', label: 'Description' },
    { key: 'permissions', label: 'Permissions', align: 'center', formatter: (v) => `${(v as string[] ?? []).length}` },
    { key: 'isActive', label: 'Status', type: 'badge', align: 'center', formatter: (v) => (v ? 'Active' : 'Inactive') },
  ];

  readonly rowActions: TableAction<SecurityRole>[] = [
    { key: 'edit', label: 'Edit', icon: 'edit', color: 'primary', permission: ROLE_PERMISSIONS.update },
    { key: 'delete', label: 'Delete', icon: 'delete', color: 'warn', permission: ROLE_PERMISSIONS.delete },
  ];

  readonly filterFields: FilterFieldConfig[] = [
    { key: 'isActive', label: 'Status', type: 'boolean', colSpan: 3 },
  ];

  readonly config: TableConfig = { ...TABLE_DEFAULTS };

  protected get formFields(): FormFieldConfig<SecurityRole>[] {
    return [
      { key: 'name', label: 'Name', type: 'text', validators: [Validators.required, Validators.maxLength(64)], colSpan: 8 },
      { key: 'isActive', label: 'Active', type: 'switch', colSpan: 4 },
      { key: 'description', label: 'Description', type: 'textarea', colSpan: 12 },
      { key: 'permissions', label: 'Permissions', type: 'multiselect', options: this.permissionOptions(), colSpan: 12 },
    ];
  }

  protected override labelOf(row: SecurityRole): string {
    return row.name;
  }

  ngOnInit(): void {
    this.service.availablePermissions().subscribe((perms) =>
      this.permissionOptions.set((perms ?? []).map((p) => ({ value: p, label: p }))));
    this.load();
  }

  onAction(event: ActionEvent<SecurityRole>): void {
    switch (event.action.key) {
      case 'edit': this.openEdit(event.context); break;
      case 'delete': this.confirmDelete(event.context); break;
    }
  }
}
