# Release Checklist — v1.0.0-rc.0

Pre-tag verification gate for **MeiCarOrt ERP Forge / ERP Platform Starter Kit**.
Executed 2026-06-07. Every item below was run and confirmed before tagging `v1.0.0-rc.0`.

## 1. Builds — 0 errors / 0 warnings

| # | Target | Command | Result |
|---|--------|---------|--------|
| 1 | Frontend app | `ng build erp-frontend --configuration production` | ✅ 0/0 (516 kB initial) |
| 2 | Angular library | `ng build erp-platform` | ✅ built `@erp-platform/core` |
| 3 | Schematics | `tsc -p projects/erp-platform/schematics/tsconfig.json` + copy-assets | ✅ compiled |
| 4 | Backend | `dotnet build ErpBackend.Api` (Api + Domain/Application/Infrastructure/CrossCutting) | ✅ 0/0 |
| 5 | CLI (`erpgen`) | `dotnet pack tools/ErpPlatform.Cli` | ✅ packed |
| 6 | NuGet (CrossCutting) | `dotnet pack ErpBackend.CrossCutting` | ✅ packed |
| 7 | Template pack | `dotnet pack packaging/ErpPlatform.Templates` | ✅ packed |

> All seven produced via `./publishing/pack-all.ps1` and standalone build commands.

## 2. Local install from `dist-packages/` (offline folder feed)

Run: `./publishing/verify-local-install.ps1` — **ALL CLIENT INSTALL STEPS PASSED**.

- ✅ `dotnet add package ErpPlatform.CrossCutting` + build (0/0)
- ✅ `dotnet tool install ErpPlatform.Cli` → `erpgen --help`
- ✅ `dotnet new install ErpPlatform.Templates`
- ✅ `dotnet new erp-module` (template smoke test)
- ✅ `npm install @erp-platform/core` (from packed `.tgz`)

## 3. Commercial demo — manifest → full stack (runtime)

Both modules generated from their `*.module.json` manifest into **frontend and backend**:

| Module | manifest→backend | manifest→frontend |
|--------|------------------|-------------------|
| Customer (`/api/sales/customers`) | ✅ generated + wired + running | ✅ generated + wired + builds |
| Product (`/api/catalog/products`) | ✅ generated + wired + running | ✅ generated + wired + builds |

Runtime API verification (API on `http://localhost:5028`, demo users `admin` / `viewer`):

| Check | Customer | Product |
|-------|----------|---------|
| Create → `201` | ✅ | ✅ |
| List → `200` | ✅ | ✅ |
| Get by id → `200` | ✅ | ✅ |
| Update → `200` | ✅ | ✅ |
| Delete → `200` (`NoContentResponse`) | ✅ | ✅ |
| Get after delete → `404` | ✅ | ✅ |
| **Permisos**: viewer create → `403` | ✅ | ✅ |
| **Permisos**: anonymous → `401` | ✅ | — |
| **Validación**: missing required → `422` envelope | ✅ | ✅ |
| **Validación**: out-of-range price → `422` | — | ✅ |

Validation envelope confirmed: `{ success:false, statusCode:422, errorCode:"VALIDATION_ERROR", correlationId }`.

## 4. Version freeze — `1.0.0-rc.0`

| Stamp | Value |
|-------|-------|
| `frontend/erp-frontend/package.json` | `1.0.0-rc.0` |
| `projects/erp-platform/package.json` | `1.0.0-rc.0` |
| `backend/ErpBackend/Directory.Build.props` `<Version>` (drives all 3 NuGet packages) | `1.0.0-rc.0` |
| Packed artifacts | `erp-platform-core-1.0.0-rc.0.tgz`, `ErpPlatform.{CrossCutting,Cli,Templates}.1.0.0-rc.0.nupkg` |

No stray `PackageVersion` / `VersionPrefix` / `AssemblyVersion` overrides found.

## 5. Secrets / hygiene

- ✅ No tokens or secrets committed; `.npmrc` / `nuget.config` / `*.token` / `.env` gitignored.
- ✅ Only `.npmrc.example` and `nuget.config.example` committed.
- ✅ `dist-packages/`, `*.tgz`, `*.nupkg`, build output gitignored.

## 6. Constraints honored

- ✅ No new features (Product frontend was a **regeneration** from its existing manifest, not new code).
- ✅ No package rename (technical placeholders retained).
- ✅ No publishing to public registries.
- ✅ No functional-logic changes (only the verify script's source-arg + exit-code assertions were fixed).

## 7. Tag

- [x] Repository initialized under git (`.gitignore` verified — 0 secrets/node_modules/build output tracked; 370 files).
- [x] RC commit created (`Release candidate v1.0.0-rc.0`).
- [x] Annotated tag **`v1.0.0-rc.0`** created (`git describe` → `v1.0.0-rc.0`).
- [ ] Push to a **private** remote (`git push && git push --tags`) — owner: business.

> Freeze policy: after tagging, `1.0.0-rc.0` is immutable. Any change → new tag `v1.0.0-rc.1`.
