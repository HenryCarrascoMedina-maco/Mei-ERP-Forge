# Changelog

All notable changes to the ERP Framework. Format based on [Keep a Changelog](https://keepachangelog.com);
versioning follows [SemVer](https://semver.org).

## [1.0.0] — 2026-07-05 — productization cleanup + professional CLI

A clean, sellable base with **one clear way to build a module** (the generator). Promotes the
1.0.0-rc line to a stable **1.0.0**. Open items tracked for later releases (documented as extension
points, not blockers): Excel/PDF export, a DB-backed audit trail, and external secret management for
`Jwt:SecretKey`.

### Changed
- **Single module story.** Removed the legacy hand-written Phase-13 in-memory sample modules
  (`Samples/Catalog`, `Samples/Management`, `InMemoryRepository`, `SampleProductsController`; frontend
  `catalogs`/`management`/`settings`/`users-demo`) that competed with the code-generation path. The
  generated **Customer** and **Product** modules remain as the reference examples; `security`
  (users/roles) and the home `dashboard` are kept. Routes/menu trimmed accordingly. The runtime
  low-code Builder experiment was moved out of the main line (archived).
- **Line endings.** Added `.gitattributes` (LF) and `erpgen` now normalizes generated output to LF,
  so regeneration is byte-stable (no phantom CRLF/LF diff on Windows).

### Added — `erpgen` CLI, professional DX (no architecture change, backward compatible)
- **Pre-generation validation** with actionable, per-field messages (PascalCase/kebab-case, camelCase,
  reserved-collision fields, unsupported types, missing select options, duplicates) + an
  `erpgen validate <manifest>` command. Mirrors the JSON Schema rules; no new dependency.
- **Fewer steps**: positional manifest, and auto-detection of `--project` (Program.cs anchor) and
  `--namespace` (from the `.csproj`); explicit flags still win.
- **Rich summary + ordered next steps**: files created/overwritten/kept, API route, permission keys,
  Program.cs registration (loud warning when the anchor is missing), the EF migration command and the
  permission-grant guidance.
- **`--dry-run`** (preview, writes nothing) and overwrite **confirmation** (`--yes`/`-y`/`--force`);
  CI / piped runs proceed automatically. Per-command `--help` with examples. `.config/dotnet-tools.json`
  local tool manifest (`dotnet tool restore` → `dotnet erpgen`).

### Added — rebranding (low-impact, no framework rename)
- **`src/app/branding.ts`** — single source of truth for the user-facing identity (app name, short
  name, description), wired into the toolbar, the login screen and the browser tab.
- **`docs/REBRANDING.md`** — quick rebrand + per-deployment config + the optional package/namespace
  rename (advanced). **`publishing/rebrand.ps1`** — dry-run-by-default helper that sets the product
  name across `branding.ts`, `index.html` and `Directory.Build.props`.

### Fixed
- **Permission reconcile (framework).** The Administrator role now reconciles permissions on startup —
  newly declared permissions (e.g. a generated module added to `Identity:AdditionalAdminPermissions`)
  are granted on the next boot, append-only and idempotent, without touching custom roles. Previously a
  freshly generated module returned 403 until the admin role was edited by hand.

## [1.0.0-rc.2] — Real authentication & authorization (commercial product push, L1-2)

Database-backed identity replacing the in-memory demo auth, with full user/role management — without
changing the manifest schema, the permission-claim authorization, or breaking the HTTP contract.

### Added
- **`ErpPlatform.Identity`** — new package (project `ErpBackend.Identity`). Lightweight, persistent
  identity built on CrossCutting (PasswordHelper/JwtHelper/claims) + Persistence (EF Core):
  - Entities `User` / `Role` / `UserRole` / `RefreshToken` (D3 hybrid: relational user↔role,
    `Role.Permissions` as a JSON collection). `ApplyErpIdentity(modelBuilder)`.
  - `AuthService` — real login, **persisted refresh-token rotation** (hashed at rest) and logout.
  - `UserService` / `RoleService` — CRUD, role assignment, permission editing; permission catalog.
  - `AuthController` (`/api/auth/token|refresh|logout|me`), `UsersController` / `RolesController`
    (`/api/security/users|roles`, permission-gated `users.*` / `roles.*`).
  - `AddErpIdentity(config)` (+ controllers application part), `DefaultIdentitySeeder` (admin + base
    roles, credentials from configuration — never hardcoded), `ErpIdentityOptions`.
- **API wiring** — `AddErpIdentity` + `ApplyErpIdentity` in the sample app; EF migration `AddIdentity`
  (`Users`/`Roles`/`UserRoles`/`RefreshTokens`); config-driven `Identity:DefaultAdmin`.
- **Frontend real auth** — login screen (form → `/api/auth/token` → `/me` hydration), `AuthService`
  (`login`/`loadCurrentUser`/`refresh`/`logout`), **refresh-on-401** interceptor, real logout; auto-login
  demo removed. Protected routes use `[authGuard, permissionGuard]`.
- **Security administration UI** — `features/security/users` and `features/security/roles`, composed
  entirely from the shared generic components + `CrudListBase`.
- **`GenericForm` `multiselect` field type** (reusable enhancement) — used by the user-roles and
  role-permissions pickers.
- `docs/AUTHENTICATION.md`; `pack-all.ps1` packs `ErpPlatform.Identity`; the offline gate
  (`verify-local-install`) now also installs + integrates Identity.

### Changed
- The in-memory demo `AuthController` was removed; `/api/auth/token` is now served by the Identity
  package (response additively gains `refreshToken`). Existing permission authorization unchanged.
- Version bumped to **1.0.0-rc.2** (frontend app + `@erp-platform/core` + backend packages).

### Notes
- Passwords (PBKDF2) and refresh tokens (SHA-256) are stored hashed; users/roles survive restart.
- Out of scope by design: public registration, password reset, 2FA, OAuth, advanced lockout,
  tenant management, DB-managed Permission/RolePermission table.

## [1.0.0-rc.1] — EF Core persistence (commercial product push, L1-1)

First step of the commercial productization: the demo now has **real, persistent storage** (data
survives restarts), without changing the manifest schema, the HTTP contract or the frontend.

### Added
- **`ErpPlatform.Persistence`** — new package (project `ErpBackend.Persistence`). Async-first EF Core
  layer that keeps `ErpPlatform.CrossCutting` persistence-agnostic: `ErpDbContext` (global soft-delete
  query filter + `decimal(18,2)` convention), generic `EfRepository<T>`/`IRepository<T>`,
  `AuditSaveChangesInterceptor` (CreatedBy/UpdatedBy + soft-delete), async pagination
  `ToPagedResultAsync`, provider-agnostic `AddErpPersistence<TContext>(config, configure?)`
  (SQLite in-box; SQL Server/PostgreSQL design-ready), and an idempotent seeding pipeline
  (`IErpDataSeeder` + `SeedErpDataAsync`) plus `Migrate/InitializeErpDatabaseAsync` startup helpers.
- **API persistence wiring** — `AppDbContext : ErpDbContext` (auto-discovers module mappings via
  `ApplyConfigurationsFromAssembly`), `AppDbContextFactory` (design-time for `dotnet ef`), SQLite
  config (`Database:Provider`, `MigrateOnStartup`, `ConnectionStrings:Default`), and
  `InitializeErpDatabaseAsync` at startup. Initial EF migration (`InitialCreate`) for Customer + Product.
- **Generator (`erpgen`) `--store ef|inmemory`** (default `ef`): emits `{Name}Configuration` +
  `{Name}EfRepository`, an async controller (`ToPagedResultAsync`, awaited repo calls), and an
  EF-translatable search predicate. `--store inmemory` keeps the async ConcurrentDictionary repo for
  prototyping/tests.
- `docs/PERSISTENCE.md` (providers, connection strings, `dotnet ef`, per-provider migrations,
  SQL Server/PostgreSQL, Docker readiness, search strategy).

### Changed
- Demo modules **Customer** and **Product** migrated from in-memory to EF Core (regenerated with
  `--store ef`; their `*InMemoryRepository.cs` removed). Same HTTP contract, same responses.
- Repository contract is now async (`GetAsync`/`AddAsync`/`UpdateAsync`/`RemoveAsync`); generated
  controllers are async end-to-end.
- `publishing/pack-all.ps1` now also packs `ErpPlatform.Persistence`.
- Version bumped to **1.0.0-rc.1** (frontend app + `@erp-platform/core` + backend packages).

### Notes
- Frontend unchanged (HTTP-decoupled). Manifest schema v1.0 unchanged (backward compatible).
- Auth users remain in-memory demo seed; **real auth + seed data are the next step (L1-2)**.
- Migrations are SQLite-specific; SQL Server/PostgreSQL need their own migration set.

## [Unreleased] — platform productization (post-rc)

### Added
- **Module manifest + generator (Slice 0 + 1)**: JSON Schema (Draft 2020-12)
  `module-manifest.schema.json` (FROZEN v1.0), Ajv validator, and the `module` Angular schematic that
  generates a full frontend module (model/service/list page/route/menu/permissions) from a
  `<Module>.module.json`, reusing the library + `CrudListBase`. Idempotent route/menu patches.
- **Packaging P0a-3**: the toolchain now ships **inside `@erp-platform/core`**
  (`projects/erp-platform/schematics`): `collection.json`, the `module` generator, a new **`ng-add`**
  schematic (wires `provideErpPlatform` + HttpClient/interceptors + animations via `addRootProvider`,
  idempotent), the manifest schema and the validator. Built into `dist/erp-platform/schematics` and
  shipped in `npm pack`. Consumers run `ng add @erp-platform/core` and
  `ng generate @erp-platform/core:module --manifest=...`. (`tools/erp-platform` removed.)
- **Packaging P0a-1**: extracted `shared/` into an Angular library **`@erp-platform/core`**
  (`projects/erp-platform`) with a `public-api.ts` contract. Decoupled from the app `environment` via
  `provideErpPlatform({ apiUrl })` / `ERP_PLATFORM_CONFIG`. Dev path-mapping points at library source
  (tree-shaking preserved, initial bundle ~516 kB). Docs: `docs/PACKAGING.md`, `docs/COMMERCIAL-DEMO.md`.
- **Packaging P0a-2**: migrated all app/feature imports (93 imports across 16 files) and the generator's
  output to import `@erp-platform/core` directly; removed the temporary shim tree (`src/app/shared`) and
  migration scripts. The app now consumes the library as a real package.

- **Packaging P0a-4 (backend NuGet)**: `ErpBackend.CrossCutting` now packs as **`ErpPlatform.CrossCutting`**
  (`1.0.0-rc.0`) with `IsPackable`, `GenerateDocumentationFile` (XML docs shipped, CS1591 suppressed),
  full metadata, `LICENSE.txt` + `README.md`, and a `PUBLIC-API.md` contract. Assembly/namespaces stay
  `ErpBackend.*` (rename deferred). `dotnet pack` → `ErpPlatform.CrossCutting.1.0.0-rc.0.nupkg` (lib +
  XML docs + license + readme). Scaffolded the `ErpPlatform.Templates` dotnet-new template pack (base
  only; content in Slice 2).

- **Slice 2 — backend generator (full-stack from one manifest)**:
  - `erpgen` (`ErpPlatform.Cli` dotnet tool) reads `<Module>.module.json` and generates a backend
    module — entity, DTOs + DataAnnotations, permission constants, repository interface + editable
    in-memory implementation, controller (`[HasPermission]`, `ApiResponse`/`PagedResponse`,
    `ApplyPagination`), and a DI extension — on top of `ErpPlatform.CrossCutting`. Regeneration-safe
    (`*.Generated.cs` regenerated; `*InMemoryRepository.cs` kept; `Program.cs` patched idempotently).
  - `AddErpApiValidation()` added to CrossCutting (ModelState → `ValidationException` → `ValidationErrorResponse`).
  - `ErpPlatform.Templates` filled with a token-based `dotnet new erp-module` skeleton (packs/installs/generates).
  - **Full-stack demo verified** end-to-end: `Customer.module.json` → frontend module + `erpgen` backend →
    CRUD on `/api/sales/customers` (create 201, list/get/update/delete, 403 for viewer, 422 validation envelope).

### Notes
- Build status: `ng build erp-platform` + app build clean; `dotnet build` 0 warnings / 0 errors;
  `dotnet pack` (CrossCutting + Templates) and the erpgen-generated module build without warnings.

## [1.0.0-rc] — 2026-06-07

First release candidate. The reusable ERP framework (Angular 19 + .NET 10) is feature-complete for
its v1.0 scope, validated end-to-end, and frozen for stabilization. See
[ERP-Framework-Assessment-Report.md](ERP-Framework-Assessment-Report.md) (maturity 91/100).

### Features implemented
- **Standard contracts**: `ApiResponse<T>`, `PagedResponse<T>`, `ErrorResponse`/`ValidationErrorResponse`,
  `CreatedResponse`, `NoContentResponse`; pagination/sort/filter (`PaginationParams`, `PagedResult`,
  `FilterParams`, `QueryableExtensions`) aligned across front & back.
- **Errors & observability**: global exception middleware, correlation id, request-logging,
  performance (slow-request) middleware, audit service.
- **Security**: PBKDF2 password hashing + policy, JWT issuance/validation (optional/configurable),
  permission-based authorization via dynamic policies (`[HasPermission]`), roles/claims helpers.
- **Files**: file storage service, CSV export (`IExportService`), CSV import with per-row errors
  (`IImportService`); Excel/PDF as extension points.
- **Angular shared library (20 components)**: table, form, form-dialog, actions, pagination, modal,
  confirm-dialog, empty-state, filter, status-badge, card, kpi-card, tabs, breadcrumb, page-header,
  skeleton, file-upload, export-button, import-button (+ models).
- **Angular shared infra**: services (api, base-crud, auth, storage, loading, toast, dialog,
  permission, error-handler, file, export, import), interceptors (correlation/auth/loading/error),
  guards (auth/permission/role/unsaved-changes), directives (7), validators (6+aggregator),
  pipes (7), configs (8), utils (9).
- **Sample modules** (built with only shared building blocks): Catalogs, Management, Dashboard,
  Settings, plus a kitchen-sink `/users` demo. Backend sample modules with JWT auth, permission
  authorization, CRUD, paging/sort/filter, import/export and audit.

### Architecture (final for v1.0)
- Backend layering `Api → Application/Infrastructure/CrossCutting`, `Application → Domain/CrossCutting`,
  `Infrastructure → Domain/Application/CrossCutting`, `Domain → CrossCutting`; `CrossCutting` depends on none.
- Frontend: standalone components, lazy routes, signals, `OnPush`, single `shared` barrel,
  `features/_shared/CrudListBase` for reusable CRUD logic.
- HTTP pipeline order: correlation → request-log → performance → global-exception.

### Supported functionality (validated at runtime)
CRUD · pagination · sorting · filtering · search · status management · CSV import/export ·
file upload · permission authorization (401/403) · JWT authentication · correlation id ·
audit logging · consistent error envelopes.

### Stabilization (this rc)
- Enabled `noUnusedLocals` + `noUnusedParameters` (frontend) — build verified clean.
- Removed the only duplication (multipart `FormData`) → `FileUtil.toFormData`.
- Raised the Angular initial-bundle budget for the Material baseline.
- Added cleanup, hardening, assessment reports and the full developer guide.

### Known limitations (deferred to the roadmap)
- Persistence is **in-memory** in the sample modules — no EF Core / database yet.
- No real login screen (the demo seeds session + permissions via an app initializer).
- Excel/PDF export are extension points (`NotSupportedException` / `PdfHelper.Renderer`).
- Audit is log-only (not persisted); no refresh-token rotation/storage.
- CORS dev policy is permissive; no `SecurityHeadersMiddleware`; `Jwt:SecretKey` dev secret in config.
- No automated test suite yet.

### Build status
`ng build` and `dotnet build` — 0 errors, 0 warnings.
