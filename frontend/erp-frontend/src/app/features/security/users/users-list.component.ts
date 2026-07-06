import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { Validators } from '@angular/forms';
import {
  GenericPageHeaderComponent, GenericCardComponent, GenericTableComponent, GenericFilterComponent,
  TableColumn, TableAction, TableConfig, ActionConfig, ActionEvent, FilterFieldConfig,
  FormFieldConfig, BreadcrumbItem, SelectOption, TABLE_DEFAULTS, CustomValidators,
} from '@erp-platform/core';
import { CrudListBase } from '../../_shared/crud-list.base';
import { UsersService } from './users.service';
import { SecurityUser } from './users.model';
import { RolesService } from '../roles/roles.service';

const USER_PERMISSIONS = { view: 'users.view', create: 'users.create', update: 'users.update', delete: 'users.delete' };

/** User management screen — composed entirely from shared components + CrudListBase. */
@Component({
  selector: 'app-users-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [GenericPageHeaderComponent, GenericCardComponent, GenericTableComponent, GenericFilterComponent],
  template: `
    <div class="page">
      <app-generic-page-header
        title="Users"
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
export class UsersListComponent extends CrudListBase<SecurityUser> implements OnInit {
  protected readonly service = inject(UsersService);
  private readonly rolesService = inject(RolesService);
  protected readonly entityName = 'User';

  private readonly roleOptions = signal<SelectOption[]>([]);

  readonly breadcrumbs: BreadcrumbItem[] = [
    { label: 'Home', route: '/', icon: 'home' }, { label: 'Security' }, { label: 'Users' },
  ];
  readonly newAction: ActionConfig = { key: 'create', label: 'New user', icon: 'add', permission: USER_PERMISSIONS.create };

  readonly columns: TableColumn<SecurityUser>[] = [
    { key: 'userName', label: 'Username', sortable: true, sticky: true },
    { key: 'email', label: 'Email' },
    { key: 'displayName', label: 'Name' },
    { key: 'roles', label: 'Roles', formatter: (v) => (v as string[] ?? []).join(', ') },
    { key: 'isActive', label: 'Status', type: 'badge', align: 'center', formatter: (v) => (v ? 'Active' : 'Inactive') },
  ];

  readonly rowActions: TableAction<SecurityUser>[] = [
    { key: 'edit', label: 'Edit', icon: 'edit', color: 'primary', permission: USER_PERMISSIONS.update },
    { key: 'delete', label: 'Delete', icon: 'delete', color: 'warn', permission: USER_PERMISSIONS.delete },
  ];

  readonly filterFields: FilterFieldConfig[] = [
    { key: 'isActive', label: 'Status', type: 'boolean', colSpan: 3 },
  ];

  readonly config: TableConfig = { ...TABLE_DEFAULTS };

  /** Edit/view fields (username immutable, no password here — set at creation / reset is a future feature). */
  protected get formFields(): FormFieldConfig<SecurityUser>[] {
    return [
      { key: 'email', label: 'Email', type: 'email', validators: [Validators.required, CustomValidators.email()], colSpan: 6 },
      { key: 'displayName', label: 'Display name', type: 'text', colSpan: 6 },
      { key: 'roleIds', label: 'Roles', type: 'multiselect', options: this.roleOptions(), colSpan: 12 },
      { key: 'isActive', label: 'Active', type: 'switch', colSpan: 6 },
    ];
  }

  /** Create fields add the username + initial password. */
  private createFields(): FormFieldConfig<SecurityUser>[] {
    return [
      { key: 'userName', label: 'Username', type: 'text', validators: [Validators.required, Validators.minLength(3), Validators.maxLength(64)], colSpan: 6 },
      { key: 'email', label: 'Email', type: 'email', validators: [Validators.required, CustomValidators.email()], colSpan: 6 },
      { key: 'displayName', label: 'Display name', type: 'text', colSpan: 6 },
      { key: 'password', label: 'Initial password', type: 'password', validators: [Validators.required, Validators.minLength(6)], colSpan: 6 },
      { key: 'roleIds', label: 'Roles', type: 'multiselect', options: this.roleOptions(), colSpan: 12 },
    ];
  }

  protected override labelOf(row: SecurityUser): string {
    return row.userName;
  }

  ngOnInit(): void {
    this.rolesService.list({ page: 1, pageSize: 200 }).subscribe((res) =>
      this.roleOptions.set(res.data.map((r) => ({ value: r.id, label: r.name }))));
    this.load();
  }

  /** Override to use the create-specific fields (username + password). */
  override openCreate(): void {
    this.dialog
      .openForm<SecurityUser>({ title: `New ${this.entityName}`, fields: this.createFields(), mode: 'create' })
      .subscribe((value) => {
        if (!value) return;
        this.service.create(value).subscribe(() => {
          this.toast.success(`${this.entityName} created successfully.`);
          this.load();
        });
      });
  }

  onAction(event: ActionEvent<SecurityUser>): void {
    switch (event.action.key) {
      case 'edit': this.openEdit(event.context); break;
      case 'delete': this.confirmDelete(event.context); break;
    }
  }
}
