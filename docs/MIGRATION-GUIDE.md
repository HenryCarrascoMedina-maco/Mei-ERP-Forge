# ERP Framework — Migration Guide

How to adopt this framework in an **existing** Angular 19 app and an **existing** .NET solution.
Pairs with the [Developer Guide](DEVELOPER-GUIDE.md) (how to build modules once migrated).

---

## TL;DR answers

1. **Can I copy `frontend/shared` and `ErpBackend.CrossCutting` into an existing project?**
   **Yes.** Both are self-contained reuse layers designed exactly for this. `frontend/src/app/shared`
   drops into any Angular 19 standalone app; `ErpBackend.CrossCutting` is a class library with **no
   dependency on the other backend projects** (it only references the ASP.NET Core shared framework +
   the JWT package).

2. **What must I adapt manually?**
   - Frontend: register HTTP client + interceptors + animations in `app.config.ts`; set
     `environment.apiUrl`; add Angular Material + `@angular/animations` if missing; align the
     `permission`/route strings to your domain.
   - Backend: add the project reference + `AddErpCrossCutting(Configuration)`, `AddErpJwtAuthentication`,
     `AddErpAuthorization`, and `UseErpCrossCutting()` + `UseAuthentication/UseAuthorization` in
     `Program.cs`; provide `Jwt`/`ErpLogging`/`FileStorage` config; map your real permission constants.

3. **What should I NOT copy (or replace with your own)?**
   - The generated **example modules** `Customer`/`Product` (frontend `features/customers`,
     `features/products`; backend `Modules/Customer`, `Modules/Product`) — they are **reference
     examples**. Generate your own with `erpgen` and remove them.
   - The demo `DashboardController` placeholder aggregates — point them at your real data.
   - The demo `Jwt:SecretKey` in `appsettings.Development.json` — set a real secret (user-secrets/env).
   - The seeded demo `admin`/`viewer` credentials (`Identity:DefaultAdmin`, `DemoViewerPassword`).

4. **Correct order to refactor an existing project:**
   1) Copy the reuse layers (`shared/`, `CrossCutting`). 2) Wire `app.config.ts` / `Program.cs`.
   3) Standardize **backend responses** (`ApiResponse`/`PagedResponse`) + global exception middleware.
   4) Standardize **frontend HTTP** (`ApiService`/`BaseCrudService` + interceptors).
   5) Migrate **one** module end-to-end as a pilot (table → `generic-table`, form → `generic-form`,
      actions → `generic-actions`). 6) Add **permissions/guards**. 7) Roll out to remaining modules.
   8) Add files/import/export where needed.

---

## FRONTEND migration

### 1. Which folder to copy
Copy the entire **`frontend/erp-frontend/src/app/shared/`** folder. Optionally also copy
`src/environments/` if you don't have environment files.

### 2. Where to paste it
Into your Angular app at **`src/app/shared/`**. Import via relative paths or add a path alias
(`"@shared/*": ["src/app/shared/*"]`) in `tsconfig.json` `compilerOptions.paths`.

### 3. Dependencies to install
```bash
npm i @angular/material @angular/cdk @angular/animations
# (Angular 19.x core/router/forms/common are already in an Angular 19 app)
```
Recommended `tsconfig` strictness (already used here): `strict`, `noUnusedLocals`,
`noUnusedParameters`, Angular `strictTemplates`.

### 4. Register in `app.config.ts`
```ts
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  correlationIdInterceptor, authInterceptor, loadingInterceptor, errorInterceptor,
} from './shared/interceptors';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([
      correlationIdInterceptor,   // X-Correlation-Id
      authInterceptor,            // Bearer token
      loadingInterceptor,         // global loading counter
      errorInterceptor,           // toast + 401 logout
    ])),
  ],
};
```
Set `src/environments/environment.ts` → `apiUrl: 'https://your-api/api'`.

### 5. Interceptors
They are **functional** interceptors registered in the `withInterceptors([...])` array above (order
matters: correlation → auth → loading → error). Opt a request out of auth/loading with the context
tokens `SKIP_AUTH` / `SKIP_LOADING`.

### 6. Using `shared/components`
All are standalone — import the component class into your component's `imports: [...]` (or import
from the `shared` barrel) and use the selector. They are config-driven (typed `@Input()`s/`@Output()`s).

### 7. Migrate an existing table → `generic-table`
Replace your bespoke `<table>`/`mat-table` with column config + the component:
```ts
columns: TableColumn<Order>[] = [
  { key: 'number', label: 'Order #', sortable: true, sticky: true },
  { key: 'total', label: 'Total', type: 'currency', align: 'right' },
  { key: 'status', label: 'Status', type: 'badge', formatter: v => String(v) },
];
rowActions: TableAction<Order>[] = [{ key: 'edit', label: 'Edit', icon: 'edit', permission: 'orders.update' }];
```
```html
<app-generic-table [columns]="columns" [data]="rows()" [rowActions]="rowActions"
  [loading]="loading()" [page]="page" [pageSize]="pageSize" [totalItems]="total()"
  (pageChange)="onPage($event)" (sortChange)="onSort($event)" (actionClick)="onAction($event)" />
```
Move your data-loading into a `BaseCrudService` subclass (or extend `CrudListBase` to get
load/page/sort/filter/CRUD for free).

### 8. Migrate an existing form → `generic-form`
Replace hand-built reactive forms with `FormFieldConfig[]`:
```ts
fields: FormFieldConfig[] = [
  { key: 'name', label: 'Name', type: 'text', validators: [CustomValidators.notBlank()], colSpan: 6 },
  { key: 'email', label: 'Email', type: 'email', validators: [Validators.required, CustomValidators.email()], colSpan: 6 },
];
```
```html
<app-generic-form [fields]="fields" [value]="model" mode="edit" (formSubmit)="save($event)" />
```
For dialogs use `DialogService.openForm({ title, fields, mode, value })`.

### 9. Migrate existing actions → `generic-actions`
```ts
actions: ActionConfig<Order>[] = [
  { key: 'approve', label: 'Approve', icon: 'check', permission: 'orders.approve' },
  { key: 'delete', label: 'Delete', icon: 'delete', color: 'warn', requiresConfirmation: true },
];
```
```html
<app-generic-actions [actions]="actions" [context]="row" mode="menu" (actionClick)="onAction($event)" />
```
Permission/hidden/disabled are honored automatically (via `PermissionService`).

### 10. Guards / directives / pipes / validators / utils
- **Guards** (functional): `authGuard`, `permissionGuard` (`data.permission`), `roleGuard`
  (`data.roles`), `unsavedChangesGuard`. Add to `canActivate`/`canDeactivate` in routes.
- **Directives**: `*appHasPermission`, `appPreventDoubleClick`, `appOnlyNumbers`, `appUppercase`,
  `appLowercase`, `appTrimInput`, `appAutofocus` — import into the component's `imports`.
- **Pipes**: `dateFormat`, `currencyFormat`, `booleanLabel`, `statusLabel`, `truncate`, `enumLabel`,
  `fileSize` — import the pipe class, use in templates.
- **Validators**: `CustomValidators.*` (+ `email`/`password`/`dateRange`/`file`/`duplicate`).
- **Utils** (static): `DateUtil`, `StringUtil`, `NumberUtil`, `FileUtil`, `ObjectUtil`, `ArrayUtil`,
  `FormUtil`, `RouteUtil`, `ExportUtil` — call directly.
- Populate `PermissionService.setPermissions([...])` after your real login.

---

## BACKEND migration

### 1. Which project to copy
Copy the **`ErpBackend.CrossCutting`** project folder into your solution directory.

### 2. Add it to an existing solution
```bash
dotnet sln add ./ErpBackend.CrossCutting/ErpBackend.CrossCutting.csproj
```
It targets `net10.0`, references the ASP.NET Core shared framework and
`Microsoft.AspNetCore.Authentication.JwtBearer`. If your solution uses a different namespace prefix,
rename `ErpBackend.CrossCutting` consistently (folder, csproj, root namespace).

### 3. References to add
From your **API/Web** project:
```bash
dotnet add ./YourApi/YourApi.csproj reference ./ErpBackend.CrossCutting/ErpBackend.CrossCutting.csproj
```
Other layers (Application/Infrastructure/Domain) may reference CrossCutting as needed; CrossCutting
must reference none of them.

### 4. Register in `Program.cs`
```csharp
using ErpBackend.CrossCutting.Extensions;

builder.Services.AddControllers();
builder.Services.AddErpCrossCutting(builder.Configuration);   // current-user, file/export/import, audit, logging options
builder.Services.AddErpJwtAuthentication(builder.Configuration); // optional: only enforced if Jwt:SecretKey is set
builder.Services.AddErpAuthorization();                       // [HasPermission] dynamic policies

var app = builder.Build();
app.UseErpCrossCutting();   // correlation → request-log → performance → global-exception
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```
Config sections to provide: `Jwt` (Issuer/Audience/SecretKey/expiry), `ErpLogging`
(EnableRequestLogging/SlowRequestThresholdMs), `FileStorage` (BasePath).

### 5. Using the cross-cutting pieces
- **Responses**: return `ApiResponse<T>`/`PagedResponse<T>` (or derive controllers from
  `BaseApiController` for `OkResponse`/`PagedOk`/`CreatedResponse`/`NoContentResponse`).
- **Errors**: throw `NotFoundException`/`ValidationException`/`ConflictException`/`BusinessException`/
  `UnauthorizedException`/`ForbiddenException` → `GlobalExceptionMiddleware` formats them.
- **CorrelationId**: automatic (header in/out, in logs and error envelopes).
- **JWT**: issue tokens with `JwtHelper.GenerateToken(ClaimsHelper.BuildClaims(...), settings)`;
  validation is automatic once `Jwt:SecretKey` is set.
- **Permissions**: annotate endpoints with `[HasPermission("module.action")]`; read claims via
  `ICurrentUserService` / `ClaimsPrincipalExtensions`.
- **Logging/Audit**: request/performance logging is automatic; inject `IAuditService` and call
  `LogAsync("Updated", "Entity", id, changes)`.
- **Import/Export**: inject `IExportService.ToCsv(...)` / `IImportService.ImportCsv(...)`.
- **File storage**: inject `IFileStorageService.SaveAsync/ReadAsync/DeleteAsync`; validate with `FileHelper`.
- **Pagination/sorting**: `query.ApplyPagination(paginationParams)` (or `ApplySort` + `ToPagedResult`).

### 6. Migrate an existing controller
Before:
```csharp
[HttpGet]
public async Task<IActionResult> Get(int page, int size) {
    var items = await _repo.GetPage(page, size);
    return Ok(items);   // ad-hoc shape
}
```
After:
```csharp
[Route("api/orders")]
public class OrdersController(IExportService export) : BaseApiController
{
    [HttpGet]
    [HasPermission("orders.view")]
    public ActionResult<PagedResponse<OrderDto>> GetAll([FromQuery] OrderFilter filter)
    {
        var query = _repo.Query();                       // IQueryable<Order> (EF or in-memory)
        if (!string.IsNullOrWhiteSpace(filter.Search)) query = query.Where(/* ... */);
        var paged = query.ApplyPagination(filter);       // shared sort + page
        var dtos = new PagedResult<OrderDto>(paged.Items.Select(Map).ToList(),
                       paged.Meta.Page, paged.Meta.PageSize, paged.Meta.TotalItems);
        return PagedOk(dtos);
    }
}
```
`OrderFilter : FilterParams`, `OrderDto : BaseDto`. Errors become exceptions; the envelope and
correlation id come for free.

---

## What NOT to copy / replace with your own (recap)
- The generated example modules `features/customers` + `features/products` (frontend) and
  `Modules/Customer` + `Modules/Product` (backend) — generate your own with `erpgen`, then remove them.
- The demo `DashboardController` placeholder data — point it at your real modules.
- The dev `Jwt:SecretKey` and the seeded `admin`/`viewer` demo credentials.
- Reports/checklists meant for this template’s own assessment.

## What to adapt
- Generate your domain modules with `erpgen` (they use the EF Core repository by default).
- Set a real `Jwt:SecretKey` (user-secrets/env/vault) and your own admin via `Identity:DefaultAdmin`.
- Grant your modules' permissions via `Identity:AdditionalAdminPermissions` (the admin role
  auto-reconciles on startup) or the Roles admin screen.
- Replace permission/role constant strings with your domain’s.
- Restrict CORS, add security headers, and move `Jwt:SecretKey` to user-secrets/env for production.
