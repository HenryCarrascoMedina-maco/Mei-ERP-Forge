# Module Manifest — Public Contract (FROZEN v1.0)

Status: **FROZEN — v1.0** · Date: 2026-06-07 · Artifact: [`schemas/module-manifest.schema.json`](schemas/module-manifest.schema.json)

The manifest is a **public product contract**. As of v1.0 its shape is frozen under the rules below.
Generators (frontend Slice 1, backend Slice 2) target this contract.

## What is frozen (MAJOR 1)
- Top-level keys: `schemaVersion`, `module`, `entity`, `permissions`, `list`, `form`, `api`, `features`,
  and the reserved sections `tenant`, `theme`, `whiteLabel`, `i18n`, `layout`, `hooks`, plus `x-extensions`.
- `entity.fields[]` field shape: `name`, `type`, `label`, `required`, `unique`, `default`,
  `validation{…}`, `options[]`, `ui{…}` (+ reserved `optionsSource`, `reference`).
- Field `type` enum and `ui.*` enums as defined in the schema.
- File convention: `<Module>.module.json`; `schemaVersion` is SemVer `MAJOR.MINOR`.

## Compatibility rules (binding)
1. **Additive-only within MAJOR 1.** New keys/enums may be added **only as optional with a default**.
2. **Never** remove, rename, or change the meaning/type of an existing key inside MAJOR 1.
3. Reserved sections may be **activated** (given behavior) additively; their typed shape won't shrink.
4. Unknown `x-*` keys are always ignored by tooling (forward compatibility).
5. Breaking changes require `schemaVersion` MAJOR bump (`2.0`) and a documented migration.
6. Generators must **migrate older minors** by filling defaults — an old manifest keeps working.

## Change process
- Propose change → classify (additive vs breaking) → if additive, bump MINOR and update schema +
  this contract + CHANGELOG → if breaking, open a v2.0 track. No silent edits to the v1.0 schema.

## Frozen examples (golden inputs)
[`examples/Customer.module.json`](examples/Customer.module.json) · [`examples/Product.module.json`](examples/Product.module.json)
— validated by `npm run validate` (2/2). These double as regression fixtures for the generators.
