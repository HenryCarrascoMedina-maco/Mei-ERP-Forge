# ERP Template

A reusable base template for building ERP applications quickly. It pairs an **Angular 19** frontend
of configuration-driven shared components with a **.NET (ASP.NET Core) Web API** organized around
clean, reusable cross-cutting concerns.

The goal is to create new ERP modules by *configuring* (table columns, form fields, actions,
DTOs, endpoints, permissions) rather than rewriting plumbing each time.

```
erp-template/
├─ frontend/erp-frontend/      # Angular 19 + Angular Material
└─ backend/ErpBackend/         # .NET solution (5 projects)
```

**Version:** `1.0.0` (see [`VERSION`](VERSION) / [CHANGELOG](CHANGELOG.md)).

### Documentation
- **[Getting Started](docs/GETTING-STARTED.md)** — install the platform and generate your first
  module (frontend + backend) in ~15 minutes. *Start here.*
- **[Commercial Install Guide](docs/COMMERCIAL-INSTALL-GUIDE.md)** — customer/company-facing install
  (MeiCarOrt ERP Forge): artifacts, private registries, credentials, licensing, support, checklist.
- **[Publishing Guide](docs/PUBLISHING.md)** — pack & publish the artifacts to private npm/NuGet
  registries; verify the client install experience offline (no secrets committed).
- **[Developer Guide](docs/DEVELOPER-GUIDE.md)** — overview, folder structure, and 10 how-to guides
  (new module, CRUD, permissions, routes, forms, tables, import/export, file upload).
- **[Persistence Guide](docs/PERSISTENCE.md)** — EF Core persistence: SQLite default, providers,
  connection strings, `dotnet ef` migrations, SQL Server/PostgreSQL, Docker readiness.
- **[Authentication Guide](docs/AUTHENTICATION.md)** — real identity (`ErpPlatform.Identity`): login,
  refresh-token rotation, JWT + permission claims, user/role management, seeding.
- **[Rebranding Guide](docs/REBRANDING.md)** — make it *yours*: one branding file, a helper script,
  and the environment config a buyer sets. *(New)*
- [Assessment Report](ERP-Framework-Assessment-Report.md) — functional audit, scores, v1.0 checklist.
- [Changelog](CHANGELOG.md) · [Roadmap](FRAMEWORK-ROADMAP.md) · [Cleanup Report](CLEANUP-REPORT.md) · [Hardening Report](HARDENING-REPORT.md)

---

## Status — v1.0.0 · framework + code generator, EF persistence, real auth

The reusable core is complete and **validated end-to-end**: standard API contracts, pagination/
sort/filter, error handling + correlation, **EF Core persistence** (`ErpPlatform.Persistence`),
**real authentication & authorization** (`ErpPlatform.Identity`: login, roles, refresh-token
rotation, permission policies), files/import/export, a 19-component Angular library
(`@erp-platform/core`), and a **manifest-driven code generator** (`erpgen` CLI + Angular schematics)
that scaffolds a full-stack module from one `<Module>.module.json`.

The repo ships two **generated example modules** — `Customer` and `Product` (built with `erpgen`) —
plus the real security admin (users/roles) and a home dashboard. Legacy hand-written demo modules
were removed so there is a single, clear way to build a module: **the generator**. Both projects
build cleanly (0 errors). See **[ERP-Framework-Assessment-Report.md](ERP-Framework-Assessment-Report.md)**.

**Backend (`ErpBackend.CrossCutting`)**
- `Common/` — `BaseEntity`, `AuditableEntity`, `SoftDeleteEntity`, base DTOs, `Result` / `Result<T>`
- `Responses/` — `ApiResponse<T>`, `ErrorResponse`, `ValidationErrorResponse`, `PagedResponse<T>`, `CreatedResponse<TKey>`, `NoContentResponse`
- `Pagination/` — `PaginationParams`, `PagedResult<T>`, `SortParams`, `FilterParams`, `PaginationMeta`
- `Exceptions/` — `AppException` + `NotFound`/`Validation`/`Unauthorized`/`Forbidden`/`Conflict`/`Business`
- `Middlewares/` — `GlobalExceptionMiddleware`, `CorrelationIdMiddleware`
- `Extensions/` — `QueryableExtensions` (dynamic sort + paginate), `ServiceCollectionExtensions`, `ApplicationBuilderExtensions`, `ClaimsPrincipalExtensions`
- `Security/` — `ICurrentUserService`/`CurrentUserService`, `PermissionConstants`, `RoleConstants`, `ClaimsConstants`; **+** `JwtSettings`, `PasswordPolicy`, `PermissionRequirement`, `PermissionHandler`, `PermissionPolicyProvider`, `[HasPermission]` attribute
- `Constants/` — `HttpConstants`, `ErrorMessages`, `SuccessMessages`
- `Helpers/` — `FileHelper`, `StringHelper`, `DateTimeHelper`, `PasswordHelper` (PBKDF2), `ClaimsHelper`, `CsvHelper`, `JwtHelper`, `PdfHelper` (extension point)
- `Extensions/` — **+** `AuthenticationExtensions` (`AddErpJwtAuthentication`), `AuthorizationExtensions` (`AddErpAuthorization`)
- `Middlewares/` — **+** `RequestLoggingMiddleware`, `PerformanceMiddleware`
- `Logging/` — `LogEvent`, `LogConstants`, `LoggingOptions`, `AuditLog`, `IAuditService`/`AuditService`
- `Utilities/` — `IFileStorageService`/`FileStorageService`, `IExportService`/`ExportService` (CSV; Excel/PDF extension points), `IImportService`/`ImportService`

**Frontend (`src/app/shared`)**
- `models/` — `ApiResponse`, `PagedResponse`, `PaginationRequest`, `Sort`, `Filter`, `SelectOption`, `FileMetadata`, `ImportResult` (mirror the backend contracts)
- `services/` — `ApiService`, `BaseCrudService`, `AuthService`, `StorageService`, `LoadingService`, `ToastService`, `DialogService`, `PermissionService`, `ErrorHandlerService`, `FileService`, `ExportService`, `ImportService`
- `interceptors/` — `correlationId`, `auth`, `loading`, `error`
- `components/` (core) — `generic-table`, `generic-form`, `generic-actions`, `generic-pagination`, `generic-modal`, `generic-confirm-dialog`, `generic-empty-state`
- `components/` (layout, Phase 10) — `generic-status-badge`, `generic-card`, `generic-kpi-card`, `generic-tabs`, `generic-breadcrumb`, `generic-page-header`, `generic-skeleton`
- `components/` (files, Phase 12) — `generic-file-upload`, `generic-export-button`, `generic-import-button`
- `components/` (Phase 13) — `generic-filter` (config-driven filter bar), `generic-form-dialog` (modal + form, used by `DialogService.openForm`)
- `directives/` — `has-permission`, `prevent-double-click`, `only-numbers`, `uppercase`, `lowercase`, `trim-input`, `autofocus` *(Phase 11)*
- `guards/` — `auth`, `permission`, `role`, `unsaved-changes` *(Phase 11)*
- `validators/` — `CustomValidators` + `email`, `password`, `date-range`, `file`, `duplicate` *(Phase 11)*
- `pipes/` — `dateFormat`, `currencyFormat`, `booleanLabel`, `statusLabel`, `truncate`, `enumLabel`, `fileSize`
- `configs/` — `PAGINATION_CONFIG`, `TABLE_DEFAULTS`, `FORM_DEFAULTS`, `ROUTE_PERMISSIONS`, `MENU_CONFIG`, `VALIDATION_MESSAGES`, `APP_ROUTES`, `THEME_CONFIG`
- `utils/` — `DateUtil`, `StringUtil`, `NumberUtil`, `FileUtil`, `ObjectUtil`, `ArrayUtil`, `FormUtil`, `RouteUtil`, `ExportUtil`

---

## Getting started

### Backend
```bash
cd backend/ErpBackend
dotnet build
dotnet run --project ErpBackend.Api
```

Endpoints (JWT-protected; get a token first):
- `POST /api/auth/token` — body `{ "username": "admin", "password": "Admin123!" }` (or `viewer`/`Viewer123!`).
- `GET /api/sales/customers?page=1&pageSize=10&sortBy=name` — the generated **Customer** module:
  paged/sortable/filterable, `[HasPermission("customers.view")]`, EF-backed. Also `Product`.
- `GET /api/security/users` · `…/roles` — real identity administration (`ErpPlatform.Identity`).
- `GET /api/dashboard/stats` — aggregated KPIs for the home dashboard (`[Authorize]`).

> JWT is enabled in `appsettings.Development.json` (dev secret). The frontend has a **real login
> screen** — sign in as `admin` / `Admin123!`. Default route after login is `/dashboard`; the top
> nav switches modules. Data is persisted (SQLite by default), so it survives restarts.

### Frontend
```bash
cd frontend/erp-frontend
npm install
npm start
```

Open the app and sign in (`admin` / `Admin123!`). After login you land on the **dashboard** (KPI
cards + status summary); the top nav switches to the generated **Customers** / **Products** modules
and the **Users** / **Roles** security admin — every screen built entirely from the shared
components (page-header, card, table with status-badge column, filters, form-in-modal, confirm
dialog, toasts, skeleton loaders) and driven by configuration.

The backend URL is provided to the library via `provideErpPlatform({ apiUrl })` (see
`src/app/app.config.ts`).

---

## Architecture notes

- **Backend layering**: `Api → Application/Infrastructure/CrossCutting`, `Application → Domain/CrossCutting`,
  `Infrastructure → Domain/Application/CrossCutting`, `Domain → CrossCutting`. `CrossCutting` depends on
  none of the others. `CrossCutting` references the ASP.NET Core shared framework because it hosts
  middleware and DI/builder extensions.
- **Consistent responses**: every endpoint returns `ApiResponse`/`PagedResponse`; every error flows
  through `GlobalExceptionMiddleware` into `ErrorResponse`/`ValidationErrorResponse`. Derive controllers
  from `BaseApiController` for the response helpers.
- **Consistent HTTP on the frontend**: all requests pass through the interceptor chain
  (correlation id → auth → loading → error). `ApiService` unwraps the envelopes so feature code works
  with plain data; extend `BaseCrudService` for standard modules.
- **Configuration over code**: components are driven by typed config objects
  (`TableColumn`, `TableAction`, `TableConfig`, `FormFieldConfig`, `FormSection`, `ActionConfig`) and never
  hardcode business module names.

## Phase 10 — layout components

All standalone and config-driven; reuse Angular Material; no coupling to feature code. Each lives in
`shared/components/<name>` with its own model where applicable, and is exported from `shared`.
**Used across the app** — the dashboard (KPI cards) and the generated Customers/Products list screens
(page-header, card wrapper, table with status-badge column).

### `generic-status-badge`
Resolves a status string against a built-in map (`Active`, `Inactive`, `Pending`, `Approved`,
`Rejected`, `Completed`, `Error`, `Draft`, `Archived`) or accepts a full config.

| Input | Type | Notes |
|-------|------|-------|
| `status` *(req)* | `string \| StatusBadgeConfig` | known key (case-insensitive) or `{ label, color, icon, variant }` |
| `variant` | `'soft' \| 'filled' \| 'outlined'` | default `soft` |
| `showIcon` | `boolean` | default `true` |

```html
<app-generic-status-badge status="Active" />
<app-generic-status-badge [status]="{ label: 'On hold', color: 'warning', icon: 'pause' }" variant="filled" />
```
The generic table renders this automatically for columns of `type: 'badge'`.

### `generic-card`
Container with header (icon/title/subtitle/actions), loading bar, projected content and `[card-footer]`.

| Input | Type | · | Output | Payload |
|-------|------|---|--------|---------|
| `title` / `subtitle` / `icon` | `string` | | `actionClick` | `ActionEvent` |
| `actions` | `ActionConfig[]` | header actions | | |
| `actionsMode` | `'buttons' \| 'menu'` | | | |
| `loading` | `boolean` | | | |
| `outlined` | `boolean` | | | |

```html
<app-generic-card title="Directory" subtitle="All users" icon="badge" [loading]="loading()">
  <app-generic-table ...></app-generic-table>
  <div card-footer>…</div>
</app-generic-card>
```

### `generic-kpi-card`
Dashboard metric with trend indicator, loading state and optional click.

| Input | Type | · | Output |
|-------|------|---|--------|
| `title` *(req)* | `string` | | `cardClick: void` |
| `value` | `string \| number \| null` | | |
| `icon` | `string` | | |
| `percentage` | `number` | shown next to trend | |
| `trend` | `'up' \| 'down' \| 'neutral'` | | |
| `status` | `'success'\|'warning'\|'error'\|'info'\|'neutral'\|'primary'` | accent color | |
| `loading` / `clickable` | `boolean` | | |

```html
<app-generic-kpi-card title="Active users" [value]="124" icon="group" [percentage]="12.5"
                      trend="up" status="success" [clickable]="true" (cardClick)="drill()" />
```

### `generic-tabs`
Permission-aware tabs in two modes.

| Input | Type | · | Output |
|-------|------|---|--------|
| `tabs` *(req)* | `TabItem[]` | `{ key, label, icon?, disabled?, permission?, route? }` | `tabChange: TabItem` |
| `mode` | `'content' \| 'route'` | default `content` | `selectedKeyChange: string` |
| `selectedKey` | `string` | two-way (`[(selectedKey)]`) | |

```html
<!-- content mode: host renders the active panel -->
<app-generic-tabs [tabs]="tabs" [(selectedKey)]="active">
  @switch (active) { @case ('general') { … } @case ('history') { … } }
</app-generic-tabs>

<!-- route mode: host projects a router-outlet -->
<app-generic-tabs [tabs]="routeTabs" mode="route">
  <router-outlet tabs-outlet></router-outlet>
</app-generic-tabs>
```

### `generic-breadcrumb`
Manual items or auto-built from the route tree (`data.breadcrumb`).

| Input | Type | Notes |
|-------|------|-------|
| `items` | `BreadcrumbItem[]` | `{ label, route?, icon?, disabled? }` |
| `autoFromRoute` | `boolean` | builds from `ActivatedRoute` data; ignores `items` |
| `home` | `BreadcrumbItem \| null` | prepended in auto mode (default Home) |

```html
<app-generic-breadcrumb [items]="[{ label: 'Home', route: '/' }, { label: 'Users' }]" />
<app-generic-breadcrumb [autoFromRoute]="true" />   <!-- routes: data: { breadcrumb: 'Users' } -->
```

### `generic-page-header`
Standard module header — **reuses** `generic-breadcrumb`, `generic-status-badge` and `generic-actions`.

| Input | Type | · | Output |
|-------|------|---|--------|
| `title` *(req)* | `string` | | `actionClick: ActionEvent` (primary + secondary) |
| `subtitle` | `string` | | |
| `breadcrumbs` | `BreadcrumbItem[]` | or `autoBreadcrumb: boolean` | |
| `status` | `string \| StatusBadgeConfig` | badge next to the title | |
| `primaryAction` | `ActionConfig` | permission-gated flat button | |
| `secondaryActions` | `ActionConfig[]` | three-dot menu | |

Projected content slot: `[page-header-content]` (e.g. inline filters).

```html
<app-generic-page-header
  title="Users" subtitle="Manage system users" [breadcrumbs]="crumbs" status="Active"
  [primaryAction]="{ key:'create', label:'New user', icon:'add', permission:'users.create' }"
  [secondaryActions]="[{ key:'export', label:'Export', icon:'download' }]"
  (actionClick)="onHeaderAction($event)">
</app-generic-page-header>
```

## Phase 11 — directives, guards & validators

All directives are standalone (import the class into a component's `imports` array). The generated
list screens use `*appHasPermission` and `appPreventDoubleClick` on the permission-gated primary action.

### Directives
| Directive | Selector | Purpose / usage |
|-----------|----------|-----------------|
| `HasPermissionDirective` | `*appHasPermission` | Renders the element only if the user has the permission(s). `<button *appHasPermission="'users.create'">` · `*appHasPermission="['a','b']; mode:'any'"` (reads from `PermissionService`, reacts to changes) |
| `PreventDoubleClickDirective` | `appPreventDoubleClick` | Blocks duplicate submissions; first click passes, rest are swallowed for `[throttleTime]` ms (default 1000) |
| `OnlyNumbersDirective` | `appOnlyNumbers` | Restricts input to digits. `[allowDecimals]="true"` · `[allowNegative]="true"` |
| `UppercaseDirective` | `appUppercase` | Live-uppercases input text (keeps form control in sync) |
| `LowercaseDirective` | `appLowercase` | Live-lowercases input text |
| `TrimInputDirective` | `appTrimInput` | Trims leading/trailing whitespace on blur |
| `AutofocusDirective` | `appAutofocus` | Focuses the element after view init. `[appAutofocus]="condition"` to toggle |

### Guards (functional)
| Guard | Type | Usage |
|-------|------|-------|
| `authGuard` | `CanActivateFn` | `{ canActivate: [authGuard] }` — redirects to `/login?returnUrl=...` when unauthenticated |
| `permissionGuard` | `CanActivateFn` | `{ canActivate: [permissionGuard], data: { permission: 'users.view' } }` (or array + `permissionMode: 'any'`); redirects to `/forbidden` |
| `roleGuard` | `CanActivateFn` | `{ canActivate: [roleGuard], data: { roles: ['Administrator'] } }`; redirects to `/forbidden` |
| `unsavedChangesGuard` | `CanDeactivateFn` | `{ canDeactivate: [unsavedChangesGuard] }` on a component implementing `CanComponentDeactivate` (`canDeactivate(): boolean \| Observable<boolean>`); prompts a discard-changes dialog when dirty |

> The guards assume routes named `/login` and `/forbidden` exist in the consuming app; adjust the
> redirect targets there if you use different paths.

### Validators (reactive `ValidatorFn` factories)
Import individually or via the `CustomValidators` aggregator. All ignore empty values (pair with
`Validators.required` / `CustomValidators.notBlank()` for mandatory fields).

```ts
import { Validators } from '@angular/forms';
import { CustomValidators } from './shared/validators';

this.fb.group(
  {
    name:     ['', [CustomValidators.notBlank()]],
    email:    ['', [Validators.required, CustomValidators.email()]],
    password: ['', [CustomValidators.password({ minLength: 10 })]],
    confirm:  [''],
    code:     ['', [CustomValidators.duplicate(() => this.existingCodes(), { caseInsensitive: true })]],
    avatar:   [null, [CustomValidators.file({ maxSizeMb: 5, allowedExtensions: ['png', 'jpg'] })]],
    from:     [null],
    to:       [null],
  },
  {
    validators: [
      CustomValidators.match('password', 'confirm'),   // group-level
      CustomValidators.dateRange('from', 'to'),        // group-level
    ],
  },
);
```

Error keys: `email`, `password` (object of unmet rules), `dateRange`, `file` (object of violations),
`duplicate`, `required` (`notBlank`), `onlyLetters`, `fieldsMismatch` (`match`).

## Conventions
- English for code (files, classes, components); Spanish only for end-user UI strings if needed later.
- Frontend and backend contracts stay aligned (`PaginationParams` ⇄ `PaginationRequest`, `PagedResponse` ⇄ `PagedResponse`).
- Reusable components expose clear `@Input()`s and `@Output()`s.

## Shared base — skeleton, pipes, configs, utils

All standalone / pure and tree-shakeable; exported from `shared`. **Integrated in the demo**
(skeleton on Refresh, `truncate` + `booleanLabel` pipes, `TABLE_DEFAULTS` config).

### `generic-skeleton`
| Input | Type | Notes |
|-------|------|-------|
| `type` | `'table'\|'form'\|'card'\|'dashboard'\|'detail'\|'list'` | layout shape (default `list`) |
| `rows` / `columns` | `number` | repeat counts (table/list/form) |
| `animated` | `boolean` | shimmer (default `true`) |
| `height` / `width` | `string` | optional container size |

`generic-table` shows a `table` skeleton automatically on first load (`loading` + empty data).

```html
<app-generic-skeleton type="table" [rows]="8" [columns]="5" />
<app-generic-skeleton type="card" />
```

### Pipes (standalone, pure)
| Pipe | Example | Result |
|------|---------|--------|
| `dateFormat` | `{{ d \| dateFormat:'datetime' }}` | locale date/time |
| `currencyFormat` | `{{ n \| currencyFormat:'EUR':'de-DE' }}` | `1.234,56 €` |
| `booleanLabel` | `{{ active \| booleanLabel:'Active':'Inactive' }}` | mapped label |
| `statusLabel` | `{{ 'pending' \| statusLabel }}` | `Pending` |
| `truncate` | `{{ text \| truncate:20:'…':true }}` | clipped on word boundary |
| `enumLabel` | `{{ value \| enumLabel:MyEnumLabels }}` | friendly text |
| `fileSize` | `{{ bytes \| fileSize }}` | `1.4 MB` |

### Configs
`PAGINATION_CONFIG` (page sizes), `TABLE_DEFAULTS` (`TableConfig`), `FORM_DEFAULTS`,
`ROUTE_PERMISSIONS` (path → permission), `MENU_CONFIG` (`MenuItem[]`, permission-aware),
`VALIDATION_MESSAGES` (used by `FormUtil.getErrorMessage`), `APP_ROUTES`, `THEME_CONFIG`.

```ts
config = { ...TABLE_DEFAULTS, showSelection: true };          // table
[pageSizeOptions]="PAGINATION_CONFIG.pageSizeOptions"
```

### Utils (static classes, pure)
`DateUtil` (toIso/startOfDay/addDays/diffInDays), `StringUtil` (slugify/removeAccents/initials),
`NumberUtil` (round/clamp/percentage/format), `FileUtil` (extension/size/sanitizeName),
`ObjectUtil` (deepClone/removeNullish/pick/omit), `ArrayUtil` (groupBy/sortBy/uniqueBy/chunk),
`FormUtil` (markAllTouched/getDirtyValues/applyServerErrors/getErrorMessage), `RouteUtil`
(join/withQuery/parseQuery), `ExportUtil` (downloadBlob/toCsv/downloadCsv/downloadJson).

```ts
StringUtil.slugify('Área de Ventas');          // "area-de-ventas"
ObjectUtil.removeNullish(filters, true);       // strip null/undefined/'' before a request
FormUtil.applyServerErrors(form, error.errors);// bind backend validation onto controls
ExportUtil.downloadCsv(rows, 'users.csv');
```

## Phase 12 — files, import & export

**Backend** (`CrossCutting`, registered by `AddErpCrossCutting(configuration)`):
- `FileHelper` — `GetExtension`, `IsAllowedExtension`, `IsWithinSize`, `IsAllowedContentType`, `NormalizeFileName`, `GenerateSafeFileName`.
- `IFileStorageService` / `FileStorageService` — `SaveAsync`/`ReadAsync`/`DeleteAsync`/`Exists`/`GetMetadata`; disk-backed, path-traversal-guarded, configurable via `FileStorageOptions` (section `FileStorage`).
- `IExportService` / `ExportService` — `ToCsv<T>` (reflection columns + RFC-4180 escaping + BOM); `ToExcel`/`ToPdf` throw `NotSupportedException` until a package is added (override the `virtual` methods).
- `IImportService` / `ImportService` — `ImportCsv<T>(stream, map, requiredHeaders?)` → `ImportResult<T>` (totals, items, per-row `ImportRowError`s); RFC-4180-aware parser.

```csharp
// Export
var result = exportService.ToCsv(items, new[] { new ExportColumn<Dto>("Name", x => x.Name) });
return File(result.Content, result.ContentType, result.FileName);

// Import
using var stream = file.OpenReadStream();
var res = importService.ImportCsv(stream, row => new Dto { Name = row["Name"] }, requiredHeaders: new[] { "Name" });
```

**Frontend**
- `FileService` — `upload` (multipart, progress events), `download`, `getMetadata`, `delete`.
- `ExportService` — `downloadBlob`, `exportLocalCsv` (in-memory), `exportFromApi` (server blob; `csv`/`excel`/`pdf`).
- `ImportService` — `import<T>` → `ImportResult<T>`, `downloadTemplate`, `toReadableErrors`.

| Component | Key inputs | Outputs |
|-----------|-----------|---------|
| `generic-file-upload` | `allowedExtensions`, `maxSizeMb`, `maxFiles`, `multiple`, `disabled` | `filesSelected: File[]`, `errors: string[]` — drag&drop, preview, remove |
| `generic-export-button` | `types: ExportType[]` (button or menu), `label`, `icon`, `tooltip`, `loading`, `disabled` | `exportClick: ExportType` |
| `generic-import-button` | `allowedExtensions`, `showTemplate`, `label`, `icon`, `loading`, `disabled` | `fileSelected` / `importClick: File`, `templateDownload` |

```html
<app-generic-export-button [types]="['csv','excel']" (exportClick)="onExport($event)" />
<app-generic-import-button [allowedExtensions]="['csv']" [showTemplate]="true"
  (fileSelected)="onImport($event)" (templateDownload)="downloadTemplate()" />
<app-generic-file-upload [allowedExtensions]="['png','jpg']" [maxSizeMb]="5" (filesSelected)="files = $event" />
```

## Backend completion — helpers, security, logging & audit

Registered by `builder.Services.AddErpCrossCutting(builder.Configuration)` plus
`AddErpJwtAuthentication(configuration)` and `AddErpAuthorization()`. The HTTP pipeline
(`app.UseErpCrossCutting()`) runs: correlation id → request logging → performance → global exception.

**Helpers** (static, pure): `StringHelper` (slugify/removeAccents/normalize), `DateTimeHelper`
(start/end of day-month, ranges, timezone), `PasswordHelper` (PBKDF2 `Hash`/`Verify`), `ClaimsHelper`
(`BuildClaims`), `CsvHelper` (`BuildLine`/`Parse`, shared by export/import), `JwtHelper`
(`GenerateToken`/`GenerateRefreshToken`), `FileHelper`, `PdfHelper` (extension point — set `PdfHelper.Renderer`).

### Enabling JWT authentication (optional)
Auth is **off by default** — with no secret configured the demo endpoints stay anonymous. To enable,
add a `Jwt` section to `appsettings.json` (a non-empty `SecretKey` flips it on):

```jsonc
{
  "Jwt": {
    "Issuer": "ErpBackend",
    "Audience": "ErpBackend.Client",
    "SecretKey": "replace-with-a-32+char-secret-key-kept-out-of-source-control",
    "AccessTokenExpirationMinutes": 60
  }
}
```

Issue a token after validating credentials:

```csharp
var claims = ClaimsHelper.BuildClaims(user.Id, user.Email, user.UserName,
    roles: ["Manager"], permissions: ["users.view", "users.create"]);
TokenResult token = JwtHelper.GenerateToken(claims, jwtSettings);   // token.AccessToken, token.ExpiresAtUtc
```

### Permission-based authorization
Always available (no JWT needed to compile; enforcement needs an authenticated user). Guard endpoints
with the dynamic-policy attribute — no policy pre-registration required:

```csharp
[HttpPost]
[HasPermission(PermissionConstants.Users.Create)]   // 403 unless the user holds "users.create"
public IActionResult Create(...) => ...;
```

`PermissionHandler` checks the `permission` claims via `ClaimsPrincipalExtensions.HasPermission`.
Password rules are enforced with `PasswordPolicy` (`Validate`/`IsValid`), mirrored by the frontend validator.

### Audit & logging
Inject `IAuditService` and record meaningful actions; the entry is enriched with the current user,
correlation id and client IP, then written as a structured log (swap `AuditService` for a DB-backed
implementation to persist a trail):

```csharp
public class UsersController(IAuditService audit) : BaseApiController
{
    public async Task<IActionResult> Delete(Guid id)
    {
        // ... delete ...
        await audit.LogAsync("Deleted", entityName: "User", entityId: id.ToString());
        return Ok();
    }
}
```

Every request is logged (`RequestCompleted method path → status in Nms`); requests over
`ErpLogging:SlowRequestThresholdMs` (default 1000) are logged as `SlowRequest`. Configure via the
`ErpLogging` section.

## Roadmap beyond v1.0.0
v1.0.0 ships EF Core persistence, real auth (JWT + refresh-token rotation), the `erpgen` code
generator + schematics, one-file rebranding, a baseline test suite and CI. Documented extension
points remaining for later releases (not blockers):
- **Excel/PDF export**: add a package (ClosedXML/EPPlus, QuestPDF) behind the existing extension points.
- **DB-backed audit trail** (audit is currently log-only).
- **Secret management** for `Jwt:SecretKey` (user-secrets / env / vault) — never in source control.
