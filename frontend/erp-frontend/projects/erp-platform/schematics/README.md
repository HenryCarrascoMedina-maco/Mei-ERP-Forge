# @erp-platform/cli — Module Manifest & Generator

Toolchain for the ERP Platform: the **module manifest** schema + validator (Slice 0) and the
**Angular module generator** schematic (Slice 1). Define a module once → generate the full frontend.

> Placeholder scope `@erp-platform` (npm) / `ErpPlatform.*` (NuGet). Renaming later is mechanical.

## Install
```bash
cd tools/erp-platform
npm install
npm run build      # tsc + copy schematic assets to dist/
```

## Slice 0 — manifest & validation
- **Schema**: [`schemas/module-manifest.schema.json`](schemas/module-manifest.schema.json) (JSON Schema Draft 2020-12).
- **Examples**: [`examples/Customer.module.json`](examples/Customer.module.json), [`examples/Product.module.json`](examples/Product.module.json).
- **Validate**:
```bash
npm run validate                       # validates the bundled examples
npm run validate -- ./path/My.module.json   # validate your own manifest
```
Manifests are named `<Module>.module.json`, carry a `schemaVersion` (SemVer MAJOR.MINOR), and may
use `x-*` keys anywhere for forward compatibility. Reserved sections (`tenant`, `theme`,
`whiteLabel`, `i18n`, `layout`, `hooks`) are typed but inert in the MVP.

## Slice 1 — Angular module generator
Generates a complete frontend module from a manifest, reusing the framework's `shared/` components
and `CrudListBase`, and idempotently patching routes + menu + permissions.
```bash
# run against an Angular app that already contains src/app/shared (the framework)
schematics <path-to>/collection.json:module --manifest=<path>/Customer.module.json
```
Outputs under `src/app/features/<key>/`:
- `<key>.model.generated.ts`, `<key>.permissions.generated.ts`, `<key>.service.generated.ts`,
  `<key>-list.component.generated.ts` + `.html` — **regenerable** (owned by the generator).
- Patches `src/app/app.routes.ts` (lazy guarded route) and `src/app/app.menu.ts` (menu entry) at
  the `// erp-generated:routes` / `// erp-generated:menu` anchors — **idempotent** (skipped if already present).

### Regeneration safety
- Files ending in `.generated.ts` are owned by the generator and may be overwritten on re-run.
- Your editable code (custom methods, overrides) must live **outside** `.generated.*` files
  (e.g. a subclass or a `<key>.hooks.ts` you create) — the generator never overwrites non-generated files.
- Re-running with the same manifest is a no-op for patches and a clean refresh for generated files.

## Versioning / backward compatibility
- `schemaVersion` is SemVer. Within MAJOR 1, changes are **additive only** (no removals/renames).
- Unknown `x-*` keys are ignored by the generator (forward compatible).
- Reserved sections activate additively in future minors.
