# Packaging Strategy (P0a)

Turns the framework from "copied folders" into **versioned packages**. Incremental — each step ends
with a clean build. Placeholder scope `@erp-platform` (npm) / `ErpPlatform.*` (NuGet).

## Status
- ✅ **P0a-1** — `shared/` extracted into the Angular library `@erp-platform/core`.
- ✅ **P0a-2** — app fully migrated to import `@erp-platform/core` directly; shim tree removed.
- ✅ **P0a-3** — toolchain packaged **inside** `@erp-platform/core` (`projects/erp-platform/schematics`):
  `module` generator + `ng-add` + manifest schema + validator; built to `dist/erp-platform/schematics`,
  shipped in `npm pack`. `@erp-platform/core` is now **runtime + toolchain** in one installable package.
- ⏳ Next: backend NuGet (`ErpPlatform.CrossCutting`); template pack `ErpPlatform.Templates`.

## Frontend — current structure
```
frontend/erp-frontend/
├─ angular.json                         # workspace: app + library
├─ tsconfig.json                        # paths: "@erp-platform/core" -> projects/erp-platform/src/public-api.ts (dev = source)
├─ projects/erp-platform/               # 📦 @erp-platform/core (publishable Angular library)
│  ├─ ng-package.json · package.json    # name @erp-platform/core, "schematics": "./schematics/collection.json"
│  ├─ schematics/                       # 🧰 TOOLCHAIN (shipped in the package)
│  │  ├─ collection.json                # ng-add + module
│  │  ├─ ng-add/  module/               # factories (compiled to dist/erp-platform/schematics)
│  │  ├─ schemas/module-manifest.schema.json   # single source of truth
│  │  ├─ validate/  examples/           # Ajv validator + golden manifests
│  │  ├─ tsconfig.json · copy-assets.mjs       # build the schematics into dist
│  └─ src/
│     ├─ public-api.ts                  # 🔓 PUBLIC CONTRACT (config token + shared barrel)
│     └─ lib/
│        ├─ config/erp-platform.config.ts   # ERP_PLATFORM_CONFIG + provideErpPlatform (decouples from app env)
│        ├─ components/ services/ interceptors/ guards/ directives/ pipes/ validators/ configs/ utils/ models/
│        └─ index.ts                    # shared barrel
└─ src/app/                            # app code — imports `@erp-platform/core` directly (no shims)
```

### Key decisions (P0a-1)
- **Dev path-mapping points at library source** (`public-api.ts`), not `dist/`. Preserves tree-shaking
  and lazy chunks (initial bundle stayed ~516 kB) and removes the need to pre-build the lib during dev.
  `dist/` is produced by `ng build erp-platform` and used only for `npm pack` / publishing.
- **Environment decoupled**: the library never imports the app's `environment`. The host supplies
  config via `provideErpPlatform({ apiUrl })` (in `app.config.ts`), read through `ERP_PLATFORM_CONFIG`.
- **Shims were temporary** (P0a-1 only): a granular re-export tree let the app keep building while
  imports were migrated. In P0a-2 every app/feature file (and the generator's output) was switched to
  `@erp-platform/core`; the shim tree and the migration scripts were then deleted.

### Commands
```bash
# build the library
npm run ng -- build erp-platform           # -> dist/erp-platform

# build the app (consumes the library via path-mapping)
npm run build

# (future) pack for a private registry
cd dist/erp-platform && npm pack            # -> erp-platform-core-1.0.0-rc.0.tgz

# regenerate the temporary shims after moving lib files
node scripts/gen-shared-shims.mjs
```

### Public contract
`projects/erp-platform/src/public-api.ts` is the public API. It exports `provideErpPlatform` +
`ERP_PLATFORM_CONFIG` and the full shared surface (components, services, interceptors, guards,
directives, pipes, validators, configs, utils, models). Anything not re-exported there is internal.

## Backend — NuGet (P0a-4, done)
- `ErpBackend.CrossCutting` packs as **PackageId `ErpPlatform.CrossCutting`** (assembly/namespace stay
  `ErpBackend.*` — namespace rename is a separate controlled follow-up). `IsPackable`,
  `GenerateDocumentationFile` (CS1591 suppressed), full metadata, `LICENSE.txt` + `README.md` shipped.
  Public surface defined in `ErpBackend.CrossCutting/PUBLIC-API.md`.
```bash
dotnet build backend/ErpBackend/ErpBackend.slnx                       # solution build (0/0)
dotnet pack  backend/ErpBackend/ErpBackend.CrossCutting -c Release    # -> ErpPlatform.CrossCutting.1.0.0-rc.0.nupkg
# consumer:
dotnet add package ErpPlatform.CrossCutting
```
### Backend generator (Slice 2)
- **`erpgen`** (`ErpPlatform.Cli`, `backend/ErpBackend/tools/`) — `dotnet tool` that reads a
  `<Module>.module.json` and generates a full backend module (entity, DTOs + DataAnnotations,
  permissions, repository interface + editable in-memory impl, controller, DI) on top of
  `ErpPlatform.CrossCutting`. Regeneration-safe (`*.Generated.cs` regenerated; `*InMemoryRepository.cs`
  kept; `Program.cs` patched idempotently at `// erp-generated:modules`).
```bash
dotnet tool install -g ErpPlatform.Cli      # consumer (or: dotnet run --project tools/ErpPlatform.Cli --)
erpgen module --manifest Customer.module.json --project ./MyApi --namespace MyApi
```
- **`ErpPlatform.Templates`** (`dotnet new erp-module`) — token-based **skeleton** for a quick module
  start without a manifest (`--name --resource --prefix --rootNamespace`). `dotnet pack` →
  `ErpPlatform.Templates.1.0.0-rc.0.nupkg`; `dotnet new install` → `dotnet new erp-module`.
- **Validation alignment**: `AddErpApiValidation()` (CrossCutting) routes DataAnnotations/ModelState
  errors through the standard `ValidationErrorResponse`.

## Building & packing the package
```bash
npm run ng -- build erp-platform                              # 1) runtime lib -> dist/erp-platform
npx tsc -p projects/erp-platform/schematics/tsconfig.json     # 2) compile schematics -> dist/.../schematics
node projects/erp-platform/schematics/copy-assets.mjs         # 3) copy collection/schema/examples
cd dist/erp-platform && npm pack                              # -> erp-platform-core-1.0.0-rc.0.tgz
```

## How a client consumes it
```bash
npm i @erp-platform/core            # (from your private registry / tarball)
ng add @erp-platform/core           # wires providers (HttpClient+interceptors, animations, provideErpPlatform)
ng generate @erp-platform/core:module --manifest=Customer.module.json   # generate a full module
```

## Remaining P0a increments (each ends with a clean build)
1. ✅ ~~Migrate app imports → `@erp-platform/core`; delete the shim tree.~~ (P0a-2)
2. ✅ ~~Package the schematics + `ng add @erp-platform/core`.~~ (P0a-3)
3. ✅ ~~Backend: package metadata + `dotnet pack` (`ErpPlatform.CrossCutting`) + `PUBLIC-API.md`.~~ (P0a-4)
4. ✅ ~~Backend: scaffold `ErpPlatform.Templates`.~~ (P0a-4 — template content comes in Slice 2)
