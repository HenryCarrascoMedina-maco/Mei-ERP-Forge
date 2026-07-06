# Release Notes — v1.0.0

**Product:** ERP Platform Starter Kit
**Version:** `1.0.0` · **Date:** 2026-07-05 · **Status:** Stable
**Stack:** Angular 19 (standalone, signals, OnPush) + .NET 10 (ASP.NET Core)

## Since the release candidate (rc.0 → 1.0.0)
- **EF Core persistence** (`ErpPlatform.Persistence`) and **real authentication** (`ErpPlatform.Identity`:
  JWT, permission hydration, refresh-token rotation) — data and users survive restarts.
- **Manifest-driven code generator** (`erpgen` CLI + Angular schematics): one `<Module>.module.json`
  scaffolds a full-stack module. The CLI is now productized — pre-generation validation, positional
  args + project/namespace auto-detection, `--dry-run`, overwrite confirmation, a rich summary with
  ordered next steps, and `erpgen validate`.
- **Single, clear way to build a module.** Removed the legacy hand-written in-memory sample modules;
  `Customer`/`Product` (generated) are the reference examples, alongside the real users/roles admin
  and a home dashboard.
- **Permission reconcile**: the Administrator role grants newly declared permissions on startup
  (idempotent) — a generated module no longer 403s until roles are hand-edited.
- **One-file rebranding**: a single `branding.ts` drives the user-facing name (toolbar, login,
  browser tab); `docs/REBRANDING.md` + a dry-run `publishing/rebrand.ps1` helper make it a buyer's
  in minutes, with the framework packages left untouched.
- Baseline **automated tests** (18) + **CI**; LF normalization so regeneration is byte-stable.

**Open items (documented extension points / roadmap — not blockers):** Excel/PDF export
(`NotSupportedException` / `PdfHelper.Renderer` hooks), a DB-backed audit trail (audit is currently
log-only), and external secret management for `Jwt:SecretKey`.

> Packaged for **private** distribution. Package names use the technical placeholder
> (`@erp-platform/*`, `ErpPlatform.*`); a commercial rename can be applied per buyer.

---

## (Historical) Release Candidate rc.0 — 2026-06-07

---

## What this release includes

### Frontend — `@erp-platform/core` (Angular library)
- **~30 generic UI components**: table, form, form-dialog, filter, pagination, modal, confirm-dialog,
  empty-state, file-upload, import/export buttons, status-badge, card, kpi-card, tabs, breadcrumb,
  page-header, skeleton, actions.
- **Services**: api, base-crud, auth, permission, dialog, toast, loading, storage, error-handler,
  file, import, export.
- **Directives** (autofocus, has-permission, only-numbers, trim/upper/lowercase, prevent-double-click),
  **guards** (auth, permission, role, unsaved-changes), **interceptors** (auth, error, loading,
  correlation-id), **pipes**, **validators**, **utils**, and typed **models** (`ApiResponse`,
  `PagedResponse`, etc.).
- **Decoupled config** via `provideErpPlatform({ apiUrl })` + `ERP_PLATFORM_CONFIG` token.
- **Bundled schematics**: `ng add @erp-platform/core` (wires the app) and a `module` schematic that
  generates a full Angular feature (model, service, list page, route, menu, permissions) from a manifest.

### Backend — `ErpPlatform.CrossCutting` (NuGet)
- Response envelopes (`ApiResponse`/`PagedResponse`/`ErrorResponse`/`ValidationErrorResponse`),
  `QueryableExtensions.ApplyPagination`, `GlobalExceptionMiddleware`, domain exceptions.
- **JWT auth** (`AddErpJwtAuthentication`) and **permission authorization**
  (`[HasPermission("x")]` + dynamic policy provider) — both opt-in.
- **Logging/audit** helpers, `ICurrentUserService`, password/claims/JWT helpers,
  `AddErpApiValidation()` (ModelState → 422 validation envelope).
- Ships XML docs; `PUBLIC-API.md` documents the supported surface.

### Code generation — one manifest, full stack
- A single **`<Module>.module.json`** manifest (JSON Schema Draft 2020-12, frozen v1.0) drives both
  the frontend (Angular schematic) and the backend (`erpgen` dotnet tool).
- **Regeneration-safe**: `*.generated.ts` / `*.Generated.cs` are overwritten; editable files
  (e.g. `*InMemoryRepository.cs`) are created once; route/menu/DI patch points are idempotent.

### Sample modules (demo, full-stack)
- **Customer** (`/api/sales/customers`) and **Product** (`/api/catalog/products`) — each generated
  from its manifest into **both** frontend and backend, with CRUD, `[HasPermission]`, and validation.

---

## Artifacts (all `1.0.0-rc.0`)

| Artifact | Type | Package ID |
|----------|------|-----------|
| Frontend library + schematics | npm (scoped, restricted) | `@erp-platform/core` |
| Backend cross-cutting library | NuGet | `ErpPlatform.CrossCutting` |
| Backend generator CLI (`erpgen`) | NuGet (dotnet tool) | `ErpPlatform.Cli` |
| Backend module template pack | NuGet (template) | `ErpPlatform.Templates` |

Pack them with `./publishing/pack-all.ps1` → `dist-packages/`.

---

## Installation (client / consuming app)

```bash
# Frontend
npm install @erp-platform/core
ng add @erp-platform/core --api-url https://your-api/api

# Backend
dotnet add package ErpPlatform.CrossCutting
dotnet tool install ErpPlatform.Cli            # provides `erpgen`
dotnet new install ErpPlatform.Templates       # provides `dotnet new erp-module`
```

See [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md), [docs/COMMERCIAL-INSTALL-GUIDE.md](docs/COMMERCIAL-INSTALL-GUIDE.md),
and [docs/PUBLISHING.md](docs/PUBLISHING.md).

---

## Verification performed for this RC

- ✅ Builds — frontend app, Angular library, schematics, backend (Api + 4 projects), CLI, NuGet
  packages, template pack: **0 errors / 0 warnings**.
- ✅ Local install (offline folder feed): `dotnet add package`, `dotnet tool install`,
  `dotnet new install`, `dotnet new erp-module`, `npm install` — all pass.
- ✅ Runtime demo (both modules): create/list/get/update/delete, `403` (insufficient permission),
  `401` (anonymous), `422` (validation envelope) — all pass.

See [RELEASE-CHECKLIST.md](RELEASE-CHECKLIST.md) for the full verification record.

---

## Known limitations (deferred to the roadmap)

- Persistence is **in-memory** in the sample modules — no EF Core / database yet.
- No real login screen (the demo seeds session + permissions via a demo `AuthController`).
- Excel/PDF export are extension points (`NotSupportedException` / `PdfHelper.Renderer`).
- Audit is log-only (not persisted); no refresh-token rotation/storage.
- CORS dev policy is permissive; no `SecurityHeadersMiddleware`; `Jwt:SecretKey` is a dev secret in config.
- No automated test suite yet.
- Manifest features reserved for the future (multi-tenant, theming/white-label, i18n) are in the
  schema but not yet implemented.

---

## Next steps (post-RC)

1. Publish artifacts to the **private** npm/NuGet registries (see `docs/PUBLISHING.md`).
2. Pilot review / pilot sale of MeiCarOrt ERP Forge.
3. On GA: rename packages to the commercial namespace (`@meicarort/*`, `MeiCarOrt.*`) → `1.0.0`.
4. Roadmap: EF Core persistence, login UI + refresh tokens, Excel/PDF, security hardening, test suite.

> **Version policy:** once tagged, `1.0.0-rc.0` is frozen. Any further change ships as `1.0.0-rc.1`.
