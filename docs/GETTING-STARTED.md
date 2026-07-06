# ERP Platform — Getting Started

From zero to your first generated module (frontend **and** backend) in ~15 minutes. One manifest,
both stacks. No prior knowledge of the codebase required.

> **Two modes.** This guide shows the **in-repo** commands (work today, packages not yet published)
> and, where relevant, the **published-package** commands (the target UX once the private npm/NuGet
> registries are configured). In-repo commands are marked 🛠️, consumer commands 📦.

---

## 0. The flow (manifest → frontend + backend)

```
                       Customer.module.json          ← single source of truth (JSON Schema, frozen v1.0)
                                │
            ┌───────────────────┴────────────────────┐
            ▼                                          ▼
   ng generate @erp-platform/core:module        erpgen module --manifest ...
        (Angular schematic)                          (dotnet tool)
            │                                          │
            ▼                                          ▼
   src/app/features/customers/                  ErpBackend.Api/Modules/Customer/
     model · service · list page               entity · DTOs+validation · permissions
     + route + menu + permissions               repository · controller · DI ext
            │                                          │
            ▼                                          ▼
        Angular app  ───────►  HTTP  /api/sales/customers  ◄───────  ASP.NET Core API
     (consumes @erp-platform/core)                            (uses ErpPlatform.CrossCutting)
```

Generated files are **regeneration-safe**: `*.generated.ts` / `*.Generated.cs` are owned by the
tools; your custom code lives elsewhere and is never overwritten.

---

## 1. Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| Node.js | 20+ | `node -v` |
| npm | 10+ | `npm -v` |
| .NET SDK | 10.x | `dotnet --version` |
| Angular CLI | 19.x (optional; the repo ships a local one) | `ng version` |

**Estimated time**

| Step | Time |
|------|------|
| Verify prerequisites | ~3 min |
| Frontend install (`npm install`) | ~3–5 min |
| Backend restore/build | ~2 min |
| Build library + CLI | ~2 min |
| Validate + generate first module (front+back) | ~2 min |
| Build & run | ~2 min |
| **Total** | **~15 min** |

---

## 2. Frontend install

```bash
cd erp-template/frontend/erp-frontend
npm install
# Build the library (runtime) and its bundled toolchain (schematics + validator):
npm run ng -- build erp-platform
npx tsc -p projects/erp-platform/schematics/tsconfig.json
node projects/erp-platform/schematics/copy-assets.mjs
```
The app consumes `@erp-platform/core` via a TypeScript path-mapping (to the library source in dev),
so you don't need to publish anything to build the app.

📦 In a **separate** Angular 19 app once the registry is configured:
```bash
npm i @erp-platform/core
ng add @erp-platform/core      # wires HttpClient+interceptors, animations, provideErpPlatform
```

---

## 3. Backend install

```bash
cd erp-template/backend/ErpBackend
dotnet restore ErpBackend.slnx
dotnet build ErpBackend.slnx          # expect: 0 Warning(s) / 0 Error(s)
```

📦 In a separate API once the NuGet registry is configured:
```bash
dotnet add package ErpPlatform.CrossCutting
# Program.cs:
#   builder.Services.AddErpCrossCutting(builder.Configuration);
#   builder.Services.AddErpApiValidation();
#   builder.Services.AddErpJwtAuthentication(builder.Configuration);
#   builder.Services.AddErpAuthorization();
#   app.UseErpCrossCutting(); app.UseAuthentication(); app.UseAuthorization();
```

---

## 4. CLI install (backend generator `erpgen`)

🛠️ In-repo, no install needed — run it via `dotnet run`:
```bash
# from backend/ErpBackend
dotnet run --project tools/ErpPlatform.Cli -- --help
```

📦 As a tool. A local tool manifest ships in the repo (`.config/dotnet-tools.json`), so once the
package is resolvable from your feed:
```bash
dotnet tool restore        # from the repo root → provides `dotnet erpgen`
dotnet erpgen --help
```
Or install globally from a local pack:
```bash
dotnet pack tools/ErpPlatform.Cli -c Release
dotnet tool install -g ErpPlatform.Cli --add-source tools/ErpPlatform.Cli/bin/Release
erpgen --help
```

`erpgen` has three commands: `module` (generate), `validate` (check a manifest, generates nothing)
and `help`. Run `erpgen module --help` for all options.

---

## 5. Validate the manifest

A module is one file: `<Module>.module.json` (PascalCase `module.name`, kebab `module.key`,
`schemaVersion`, `entity.fields[]`, `permissions`, `api`, `features`). See
`projects/erp-platform/schematics/examples/Customer.module.json`.

```bash
# from frontend/erp-frontend
node dist/erp-platform/schematics/validate/index.js projects/erp-platform/schematics/examples/Customer.module.json
```
Expected:
```
✓ Customer.module.json — valid (schemaVersion 1.0)

1/1 manifest(s) valid.
```

The backend generator validates too (same rules), and can check a manifest without generating:
```bash
# from backend/ErpBackend
dotnet run --project tools/ErpPlatform.Cli -- validate \
  ../../frontend/erp-frontend/projects/erp-platform/schematics/examples/Customer.module.json
# → ✓ Manifest is valid — Customer · 5 field(s) · key 'customers'.
```

---

## 6. Generate the FRONTEND module

🛠️ In-repo (run from `frontend/erp-frontend`):
```bash
node node_modules/@angular-devkit/schematics-cli/bin/schematics.js \
  ./dist/erp-platform/schematics/collection.json:module \
  --manifest=projects/erp-platform/schematics/examples/Customer.module.json \
  --dry-run=false --debug=false
```
📦 Consumer: `ng generate @erp-platform/core:module --manifest=Customer.module.json`

Expected:
```
    Route added for /customers
    Menu entry added for Customers
    ✓ Generated module "Customer" in src/app/features/customers
CREATE src/app/features/customers/customers.model.generated.ts
CREATE src/app/features/customers/customers.service.generated.ts
CREATE src/app/features/customers/customers-list.component.generated.ts (+ .html, permissions)
UPDATE src/app/app.routes.ts
UPDATE src/app/app.menu.ts
```

---

## 7. Generate the BACKEND module

🛠️ In-repo (run from `backend/ErpBackend`). The manifest is positional; `--project` and
`--namespace` are auto-detected (pass them to override):
```bash
dotnet run --project tools/ErpPlatform.Cli -- module \
  ../../frontend/erp-frontend/projects/erp-platform/schematics/examples/Customer.module.json
```
📦 Consumer: `erpgen module Customer.module.json`  (add `--dry-run` first to preview, writes nothing)

Expected (abridged — erpgen prints a full summary + ordered next steps):
```
✓ Module 'Customer' generated (EF Core).
    API route : /api/sales/customers
  Program.cs: registered builder.Services.AddCustomerModule();
  Permissions declared: customers.view, customers.list, customers.create, ...
  Next steps:
    1. dotnet ef migrations add AddCustomer --project ErpBackend.Api
    2. grant the permissions to a role (Identity:AdditionalAdminPermissions, or the Roles screen)
    3. dotnet run --project ErpBackend.Api
```
Generates `Modules/Customer/` (EF Core by default): entity, DTOs (+ DataAnnotations), permissions,
`ICustomerRepository`, `{Name}Configuration` + `{Name}EfRepository`, controller (`[HasPermission]`,
`ApiResponse`/`PagedResponse`, async paging), and a DI extension; registers it in `Program.cs`.

> **EF migration:** a new module adds a new entity, so create its migration before running
> (`dotnet ef migrations add Add<Name> --project ErpBackend.Api`). For the bundled `Customer`/`Product`
> examples the migration already exists. Use `--store inmemory` to skip EF for quick prototyping.

---

## 8. Build & run

```bash
# Backend
cd erp-template/backend/ErpBackend
dotnet build ErpBackend.slnx                 # 0 Warning(s) / 0 Error(s)
dotnet run --project ErpBackend.Api --launch-profile http   # http://localhost:5028

# Frontend (new terminal)
cd erp-template/frontend/erp-frontend
npm start                                    # http://localhost:4200
```
Open `http://localhost:4200`. You land on the **login screen** — sign in with `admin` / `Admin123!`
(the seeded admin). After login the default route is `/dashboard`; the top nav switches modules
(Customers, Products, Users, Roles). Protected routes enforce the module permissions.

---

## 9. Full example with Customer (copy/paste, end-to-end)

```bash
# (1) Frontend module
cd erp-template/frontend/erp-frontend
node node_modules/@angular-devkit/schematics-cli/bin/schematics.js \
  ./dist/erp-platform/schematics/collection.json:module \
  --manifest=projects/erp-platform/schematics/examples/Customer.module.json --dry-run=false --debug=false

# (2) Backend module
cd ../../backend/ErpBackend
dotnet run --project tools/ErpPlatform.Cli -- module \
  --manifest ../../frontend/erp-frontend/projects/erp-platform/schematics/examples/Customer.module.json \
  --project ErpBackend.Api --namespace ErpBackend.Api

# (3) Build both
dotnet build ErpBackend.slnx
( cd ../../frontend/erp-frontend && npm run build )

# (4) Run the API and exercise the generated endpoints
dotnet run --project ErpBackend.Api --launch-profile http
```
Verify with a token (PowerShell):
```powershell
$base = "http://localhost:5028/api"
$login = Invoke-RestMethod "$base/auth/token" -Method Post -ContentType application/json -Body '{"username":"admin","password":"Admin123!"}'
$h = @{ Authorization = "Bearer $($login.data.accessToken)" }
Invoke-RestMethod "$base/sales/customers" -Method Post -Headers $h -ContentType application/json -Body '{"code":"C-001","name":"Acme","email":"a@erp.dev","segment":"enterprise"}'
Invoke-RestMethod "$base/sales/customers?page=1&pageSize=10&sortBy=name" -Headers $h
```
Expected (abridged): `POST` → `{ "success": true, "id": "…" }`; `GET` → `{ "data": [ … ], "meta": { "totalItems": 1, … } }`.

**Demo credentials:** `admin` / `Admin123!` (full access) · `viewer` / `Viewer123!` (read-only → 403 on writes).

---

## 10. Troubleshooting

| Symptom | Cause / Fix |
|---------|-------------|
| `ng generate @erp-platform/core:module` → "collection not found" | In-repo the package isn't installed; use the dist collection path (§6) or `npm i` the published package. |
| `validate` → `ENOENT … schemas/module-manifest.schema.json` | Run the build steps in §2 first (`tsc` + `copy-assets.mjs` populate `dist/.../schematics`). |
| Backend `403` on a generated endpoint | The role lacks the module's permission. Add `<prefix>.view/create/...` to `Identity:AdditionalAdminPermissions` and restart — the Administrator role auto-reconciles (grants new permissions on startup) — or assign them via the Roles admin screen. |
| Startup fails: `table "…" already exists` / migration mismatch | A stale dev SQLite DB from a different migration set. Delete `backend/ErpBackend/ErpBackend.Api/erp-platform*.db*` (git-ignored) and re-run to reseed a fresh DB. |
| Backend `401` everywhere | JWT is enabled (`appsettings.Development.json` has `Jwt:SecretKey`). Get a token from `POST /api/auth/token`. |
| `ng add` → "Unknown argument apiUrl" | Use kebab-case flags: `--api-url=…`. |
| `dotnet build` → "file is being used by another process (ErpBackend.Api)" | Stop the running API (`Get-Process ErpBackend.Api | Stop-Process`) before rebuilding. |
| `ng build` → "bundle initial exceeded maximum budget" | Expected with Material; budgets are set in `angular.json` (raise if needed). |
| `dotnet pack` → "Dependency ajv must be explicitly allowed" | `allowedNonPeerDependencies` is set in `ng-package.json`; only relevant if you add new non-peer deps. |
| Generated module data not persisting across requests | You generated with `--store inmemory` (a `static` store, prototyping only). Regenerate with the default EF Core store for real persistence. |
| Manifest rejected | Run the validator (§5); fix the reported field/path. `schemaVersion` must be `"MAJOR.MINOR"`. |

---

## Where to go next
- **[Developer Guide](DEVELOPER-GUIDE.md)** — build modules by hand or extend the generated ones.
- **[Packaging](PACKAGING.md)** — how the npm/NuGet packages are built and consumed.
- **[Commercial Demo](COMMERCIAL-DEMO.md)** — the "define once → full stack" sales walkthrough.
- **[Manifest Contract](../frontend/erp-frontend/projects/erp-platform/schematics/MANIFEST-CONTRACT.md)** — the frozen v1.0 schema rules.
