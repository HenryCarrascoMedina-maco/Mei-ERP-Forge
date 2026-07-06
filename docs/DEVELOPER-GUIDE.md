# ERP Framework — Developer Guide (v1.0.0-rc)

Everything a new developer needs to build a complete ERP module using only the shared framework.
Each section is self-contained and copy-pasteable. The **Categories** and **Users** sample modules
(`frontend/.../features/catalogs`, `features/management`; `backend/.../Samples/Catalog`, `Samples/Management`)
are working references for every pattern below.

**Contents**
1. [Framework Overview](#1-framework-overview)
2. [Folder Structure Guide](#2-folder-structure-guide)
3. [How To Create A New Module](#3-how-to-create-a-new-module)
4. [How To Create A New CRUD](#4-how-to-create-a-new-crud)
5. [How To Add Permissions](#5-how-to-add-permissions)
6. [How To Add Routes](#6-how-to-add-routes)
7. [How To Add Forms](#7-how-to-add-forms)
8. [How To Add Tables](#8-how-to-add-tables)
9. [How To Use Import/Export](#9-how-to-use-importexport)
10. [How To Use File Upload](#10-how-to-use-file-upload)

---

## 1. Framework Overview

A reusable base for ERP apps: **Angular 19** (standalone, signals, Material) + **.NET 10** ASP.NET Core.

**Principles**
- **One contract end-to-end** — every endpoint returns `ApiResponse<T>` or `PagedResponse<T>`; the
  frontend models mirror them exactly (`PaginationParams ⇄ PaginationRequest`).
- **Configuration over code** — tables, forms, actions, filters are driven by typed config objects.
  You build modules by configuring shared components, not writing new ones.
- **Cross-cutting is centralized** — auth, authorization, errors, correlation, logging, audit,
  files, import/export live in `CrossCutting` (backend) and `shared/` (frontend).

**Request lifecycle**
Frontend interceptors `correlationId → auth → loading → error` → HTTP →
backend pipeline `correlation → request-log → performance → global-exception` → controller
(`[HasPermission]`) → service → `IQueryable` (`QueryableExtensions`) → `PagedResponse`.

---

## 2. Folder Structure Guide

```
erp-template/
├─ backend/ErpBackend/
│  ├─ ErpBackend.Api/              # controllers, Program.cs, appsettings; Samples/ = example modules
│  ├─ ErpBackend.Application/      # (reserved) application/use-case layer
│  ├─ ErpBackend.Domain/           # (reserved) domain entities
│  ├─ ErpBackend.Infrastructure/   # (reserved) persistence (EF Core goes here — roadmap)
│  └─ ErpBackend.CrossCutting/     # SHARED backend layer (no deps on the others)
│     ├─ Common/ Responses/ Pagination/ Exceptions/ Constants/
│     ├─ Helpers/ Extensions/ Middlewares/ Security/ Logging/ Utilities/
└─ frontend/erp-frontend/src/app/
   ├─ shared/                      # SHARED frontend layer
   │  ├─ components/  (generic-* + models)   services/   interceptors/   guards/
   │  ├─ directives/  pipes/  validators/  configs/  utils/  models/
   │  └─ index.ts     (single barrel for everything)
   └─ features/                    # YOUR modules live here
      ├─ _shared/crud-list.base.ts # reusable CRUD controller base
      ├─ catalogs/  management/  dashboard/  settings/   (reference modules)
```

**Rule of thumb:** anything reusable across modules → `shared/` (frontend) or `CrossCutting/`
(backend). Anything specific to one module → that module's folder under `features/` / `Api/`.

---

## 3. How To Create A New Module

A module = a backend controller (+ DTOs) and a frontend feature folder (page + service + route).
Example: a "Products" catalog module.

**Backend** — `Api/Samples/Catalog/ProductModels.cs` + `ProductsController.cs` (see §4).
**Frontend** — `features/products/`:
- `product.model.ts` — the read model (mirror the DTO).
- `product.service.ts` — `extends BaseCrudService<Product>` with `resource = 'catalog/products'`.
- `products.component.ts/html` — page composed from shared components (see §4, §7, §8).
- a route entry (see §6) and a nav link in `app.component.ts`.

No new shared components are needed — reuse `generic-page-header`, `generic-card`,
`generic-filter`, `generic-table`, `generic-form-dialog`.

---

## 4. How To Create A New CRUD

> **Recommended: generate it.** `erpgen module <Module>.module.json` scaffolds the entire backend
> module (entity, DTOs + validation, permissions, EF repository, an async controller derived from
> `CrudControllerBase`, and DI wiring) — see [Getting Started](GETTING-STARTED.md) and the generated
> `Modules/Customer` for a complete, compiling reference. The example below shows the same shape done
> **by hand**, for when you need a custom (non-CRUD) endpoint.

### 4.1 Backend controller (by hand)
Derive from `BaseApiController`, inject your repository (the generator emits an EF Core
`{Name}EfRepository` implementing `I{Name}Repository`), and use the shared pagination/response types
and `QueryableExtensions`. The snippet below is illustrative:

```csharp
[Route("api/catalog/products")]
public class ProductsController(IExportService export) : BaseApiController
{
    private static readonly InMemoryRepository<Product> Repo = new(/* seed */);

    [HttpGet]
    [HasPermission(PermissionConstants.Catalogs.View)]
    public ActionResult<PagedResponse<ProductDto>> GetAll([FromQuery] ProductFilter filter)
    {
        var query = Repo.Query();
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase));
        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        var paged = query.ApplyPagination(filter);                    // sort + page via shared ext
        var dtos = new PagedResult<ProductDto>(
            paged.Items.Select(ToDto).ToList(), paged.Meta.Page, paged.Meta.PageSize, paged.Meta.TotalItems);
        return PagedOk(dtos);
    }

    [HttpPost]
    [HasPermission(PermissionConstants.Catalogs.Manage)]
    public ActionResult<CreatedResponse<Guid>> Create([FromBody] CreateProductDto dto)
    {
        var e = Repo.Add(new Product { Name = dto.Name, IsActive = true });
        return CreatedResponse(e.Id, SuccessMessages.Created);
    }
    // PUT {id}, DELETE {id} follow the same shape (see the generated Modules/Customer for a full,
    // compiling module — including the CrudControllerBase pattern the generator uses).
}
```
DTO shapes: `ProductDto : BaseDto` (or `BaseCatalogDto`), `CreateProductDto : BaseCreateDto`,
`UpdateProductDto : BaseUpdateDto`, `ProductFilter : FilterParams`.

### 4.2 Frontend service
```ts
@Injectable({ providedIn: 'root' })
export class ProductService extends BaseCrudService<Product> {
  protected readonly resource = 'catalog/products';   // → /api/catalog/products
}
```

### 4.3 Frontend page (extend `CrudListBase` — all CRUD logic is inherited)
```ts
export class ProductsComponent extends CrudListBase<Product> implements OnInit {
  protected readonly service = inject(ProductService);
  protected readonly entityName = 'Product';
  protected readonly formFields: FormFieldConfig<Product>[] = [ /* §7 */ ];
  readonly columns: TableColumn<Product>[] = [ /* §8 */ ];
  readonly rowActions: TableAction<Product>[] = [
    { key: 'view', label: 'View', icon: 'visibility' },
    { key: 'edit', label: 'Edit', icon: 'edit', permission: 'catalogs.manage' },
    { key: 'delete', label: 'Delete', icon: 'delete', color: 'warn', permission: 'catalogs.manage' },
  ];
  ngOnInit() { this.load(); }
  onAction(e: ActionEvent<Product>) {
    if (e.action.key === 'view') this.openView(e.context);
    else if (e.action.key === 'edit') this.openEdit(e.context);
    else if (e.action.key === 'delete') this.confirmDelete(e.context);
  }
}
```
`CrudListBase` gives you `rows()`, `total()`, `loading()`, `load()`, `onPage()`, `onSort()`,
`onFilter()`, `openCreate()`, `openEdit()`, `openView()`, `confirmDelete()` — wired to the dialogs,
toasts and your service. You only provide `service`, `entityName`, `formFields`, and the template.

---

## 5. How To Add Permissions

**Define** the constant (backend `Security/PermissionConstants.cs`):
```csharp
public static class Reports { public const string View = "reports.view"; }
```

**Protect a backend endpoint:**
```csharp
[HttpGet]
[HasPermission(PermissionConstants.Reports.View)]   // 403 unless the user's JWT carries this claim
public IActionResult Get() => ...;
```
The JWT is issued with permission claims (`ClaimsHelper.BuildClaims(..., permissions: [...])`).

**Gate frontend UI** — actions/headers honor `permission` automatically (via `PermissionService`):
```ts
{ key: 'edit', label: 'Edit', icon: 'edit', permission: 'reports.view' }   // action hidden if missing
```
Or hide arbitrary markup with the structural directive:
```html
<button *appHasPermission="'reports.view'">Run report</button>
```
Populate `PermissionService.setPermissions([...])` after login (the demo seeds it in `app.config.ts`).

---

## 6. How To Add Routes

Add a lazy route in `app.routes.ts`, optionally guarded and with breadcrumb data:
```ts
{
  path: 'catalog/products',
  canActivate: [permissionGuard],                 // or [authGuard]
  data: { permission: 'catalogs.view', breadcrumb: 'Products' },
  loadComponent: () => import('./features/products/products.component').then(m => m.ProductsComponent),
}
```
Guards available: `authGuard` (→ `/login?returnUrl`), `permissionGuard` (`data.permission`),
`roleGuard` (`data.roles`), `unsavedChangesGuard` (component implements `CanComponentDeactivate`).
Add a nav entry in `app.component.ts` `nav[]` to surface it in the toolbar.

---

## 7. How To Add Forms

Forms are config-driven. Define `FormFieldConfig[]` and render via `generic-form`, or open a dialog
with `DialogService.openForm` (used by `CrudListBase`):

```ts
protected readonly formFields: FormFieldConfig<Product>[] = [
  { key: 'name', label: 'Name', type: 'text', validators: [CustomValidators.notBlank()], colSpan: 6 },
  { key: 'price', label: 'Price', type: 'number', validators: [Validators.required], colSpan: 6 },
  { key: 'category', label: 'Category', type: 'select', options: [{ value: 1, label: 'A' }], colSpan: 6 },
  { key: 'isActive', label: 'Active', type: 'switch', colSpan: 6, defaultValue: true },
];
```
Field types: `text | number | email | password | textarea | select | autocomplete | date | checkbox
| switch | file`. Features: `validators`, `options`, `hidden`/`disabled` (static or `(value)=>bool`),
`colSpan` (1–12 grid), create/edit/**view** modes. Standalone usage:
```html
<app-generic-form [fields]="formFields" [value]="model" mode="edit" (formSubmit)="save($event)" />
```
Validation messages come from `VALIDATION_MESSAGES`; bind backend errors with
`FormUtil.applyServerErrors(form, error.errors)`.

---

## 8. How To Add Tables

Define `TableColumn[]` + `TableConfig` and bind data/state to `generic-table`:
```ts
readonly columns: TableColumn<Product>[] = [
  { key: 'name', label: 'Name', sortable: true, sticky: true },
  { key: 'price', label: 'Price', type: 'currency', align: 'right' },
  { key: 'isActive', label: 'Status', type: 'badge', align: 'center',
    formatter: v => v ? 'Active' : 'Inactive' },          // 'badge' renders generic-status-badge
  { key: 'createdAt', label: 'Created', type: 'date', sortable: true, align: 'right' },
];
readonly config: TableConfig = { ...TABLE_DEFAULTS, showSelection: true, multiSelect: true };
```
```html
<app-generic-table
  [columns]="columns" [data]="rows()" [config]="config" [rowActions]="rowActions"
  [loading]="loading()" [page]="page" [pageSize]="pageSize" [totalItems]="total()"
  (pageChange)="onPage($event)" (sortChange)="onSort($event)" (actionClick)="onAction($event)" />
```
Column types: `text | number | date | datetime | currency | boolean | badge | custom`. The table
emits server-side `pageChange`/`sortChange` (handled by `CrudListBase`), shows a skeleton on first
load and an empty state when there are no rows. Pair with `generic-filter` for search/filters:
```html
<app-generic-filter [fields]="[{ key:'isActive', label:'Status', type:'boolean' }]"
                    (filterChange)="onFilter($event)" />
```

---

## 9. How To Use Import/Export

**Export (backend)** — dogfood `IExportService` in a controller action:
```csharp
[HttpGet("export")]
[HasPermission(PermissionConstants.Catalogs.View)]
public IActionResult Export([FromQuery] ProductFilter filter)
{
    var items = Repo.Query().ApplySort(filter).ToList();
    var csv = export.ToCsv(items, new List<ExportColumn<Product>> {
        new("Name", p => p.Name), new("Price", p => p.Price),
    }, "products.csv");
    return File(csv.Content, csv.ContentType, csv.FileName);
}
```
**Export (frontend)** — a button + the shared service:
```html
<app-generic-export-button [types]="['csv']" (exportClick)="exportCsv()" />
```
```ts
exportCsv() { this.exportService.exportFromApi('catalog/products/export', 'products').subscribe(); }
// or fully client-side: this.exportService.exportLocalCsv(this.rows(), 'products.csv');
```

**Import (backend)** — `IImportService.ImportCsv` maps rows and collects per-row errors:
```csharp
[HttpPost("import")]
[HasPermission(PermissionConstants.Catalogs.Manage)]
public async Task<ActionResult<ApiResponse<ImportResult<CreateProductDto>>>> Import(IFormFile file)
{
    await using var stream = file.OpenReadStream();
    var result = importService.ImportCsv(stream,
        row => new CreateProductDto { Name = row["Name"] },
        requiredHeaders: ["Name"]);
    foreach (var dto in result.Items) Repo.Add(/* map */);
    return OkResponse(result, $"Imported {result.SuccessfulRows}/{result.TotalRows}.");
}
```
**Import (frontend)**:
```html
<app-generic-import-button [allowedExtensions]="['csv']" [showTemplate]="true"
  (fileSelected)="importCsv($event)" (templateDownload)="downloadTemplate()" />
```
```ts
importCsv(file: File) {
  this.importService.import('catalog/products/import', file).subscribe(r => {
    if (r) this.toast.success(`Imported ${r.successfulRows}/${r.totalRows}`); this.load();
  });
}
```

---

## 10. How To Use File Upload

Drop in `generic-file-upload` (selection + drag&drop + validation + preview) and upload with
`FileService`:
```html
<app-generic-file-upload
  [allowedExtensions]="['png','jpg','pdf']" [maxSizeMb]="5" [maxFiles]="3" [multiple]="true"
  (filesSelected)="files = $event" (errors)="onErrors($event)" />
```
```ts
upload(file: File) {
  this.fileService.upload('files', file).subscribe(ev => {
    if (ev.type === 'progress') this.progress = ev.progress.percent;
    else if (ev.type === 'done') this.toast.success('Uploaded');
  });
}
```
Backend: accept `IFormFile`, persist with `IFileStorageService.SaveAsync(stream, fileName)`, and
return the `StoredFileInfo` in an `ApiResponse`. Validate with `FileHelper` (extension/size/safe-name).

---

### Done — that's a full module
A new module is: DTOs + controller (`QueryableExtensions` + `[HasPermission]`) → `BaseCrudService`
subclass → page extending `CrudListBase` composed from shared components → a guarded lazy route.
No shared components are modified. See `features/catalogs` and `features/management` for complete,
working references.
