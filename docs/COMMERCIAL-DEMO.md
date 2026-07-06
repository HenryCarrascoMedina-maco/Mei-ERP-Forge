# Commercial Demo — "Define a module once, generate frontend + backend"

The headline sales demo of the ERP Platform: one `<Module>.module.json` generates a working
**frontend** module (Angular schematic) **and** a working **backend** module (the `erpgen` dotnet
tool) — both from the same file, contract-aligned by construction.

## The 5-step demo
1. **Define** a module — create `Customer.module.json` (one declarative file, no code):
   ```jsonc
   {
     "schemaVersion": "1.0",
     "module": { "name": "Customer", "key": "customers", "pluralName": "Customers",
                 "icon": "groups", "navigation": { "menuSection": "Sales" } },
     "entity": { "base": "auditable", "fields": [
       { "name": "code",  "type": "text",  "label": "Code", "required": true, "unique": true,
         "ui": { "list": true, "sortable": true, "filterable": true, "colSpan": 6 } },
       { "name": "name",  "type": "text",  "label": "Name", "required": true, "ui": { "list": true, "sortable": true, "colSpan": 6 } },
       { "name": "isActive", "type": "boolean", "label": "Status", "default": true,
         "ui": { "list": true, "badge": true, "filterable": true, "filterType": "boolean", "colSpan": 6 } }
     ] },
     "permissions": { "prefix": "customers" },
     "api": { "basePath": "sales", "resource": "customers" },
     "features": { "paginated": true, "exportable": true }
   }
   ```

2. **Validate** the manifest (validator ships inside the package):
   ```bash
   # consumer:
   node node_modules/@erp-platform/core/schematics/validate/index.js ./Customer.module.json
   ```

3. **Generate** the frontend module (one command):
   ```bash
   # consumer (after `npm i @erp-platform/core`):
   ng generate @erp-platform/core:module --manifest=Customer.module.json

   # in this repo (dev, against the freshly built package):
   node node_modules/@angular-devkit/schematics-cli/bin/schematics.js \
     ./dist/erp-platform/schematics/collection.json:module \
     --manifest=projects/erp-platform/schematics/examples/Customer.module.json --dry-run=false
   ```
   Produces `src/app/features/customers/` (`*.model/permissions/service/-list.component .generated.*`) and
   idempotently patches `app.routes.ts` + `app.menu.ts` (route, menu entry, permissions) — reusing the
   shared component library + `CrudListBase`. No hand-written code.

3b. **Generate the backend module** from the SAME manifest:
   ```bash
   # in this repo (dev): dotnet run --project backend/.../tools/ErpPlatform.Cli -- module \
   #   --manifest Customer.module.json --project backend/.../ErpBackend.Api --namespace ErpBackend.Api
   # consumer: erpgen module --manifest Customer.module.json --project ./MyApi --namespace MyApi
   ```
   Produces `Modules/Customer/` (entity, DTOs + validation, permissions, repository interface + editable
   in-memory impl, controller with `[HasPermission]` + `ApiResponse`/`PagedResponse`/`ApplyPagination`)
   and registers it in `Program.cs`. Frontend `resource` (`sales/customers`) matches the backend route
   (`/api/sales/customers`) automatically — same manifest.

4. **Run** the app:
   ```bash
   # backend:  cd backend/ErpBackend && dotnet run --project ErpBackend.Api --launch-profile http
   # frontend: cd frontend/erp-frontend && npm start
   ```

5. **See** the module working at `http://localhost:4200/customers`: a full CRUD screen — page header,
   breadcrumb, filter bar, paginated/sortable table with status badge, create/edit/view dialogs,
   delete confirmation, export — talking to the generated `/api/sales/customers` backend (CRUD +
   pagination + sort + filter + permission authorization + standard validation envelope). All from
   the one manifest, front to back.

## Why it sells
- **One source of truth** → frontend (and soon backend) generated and contract-aligned by construction.
- **Minutes, not days**, per module; zero boilerplate; consistent UX and security.
- **Regeneration-safe**: `*.generated.*` are owned by the tool; custom logic lives outside them.
- **AI/SaaS-ready**: the JSON manifest can be produced by an assistant or a future low-code UI.
