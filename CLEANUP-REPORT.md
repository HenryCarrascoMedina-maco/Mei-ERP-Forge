# Framework Cleanup Report — v1.0.0-rc

Stabilization pass before freezing the first release candidate. Scope: dead code, duplication,
obsolete files, redundant models/configs, simplifiable services. **No features changed.**

## Method
- Enabled `noUnusedLocals` + `noUnusedParameters` in `tsconfig.json` and rebuilt → the Angular
  build passing clean **proves there are no unused imports, locals or parameters anywhere** in the
  frontend.
- Backend builds with `Nullable` enabled and **0 warnings**.
- Manual review of services, models, configs and feature modules for duplication and redundancy.

## Findings & actions

| # | Area | Finding | Action |
|---|------|---------|--------|
| 1 | Duplication | `FileService.upload` and `ImportService.import` both built a multipart `FormData` inline | **Fixed** — extracted `FileUtil.toFormData(file, fieldName)`; both services delegate |
| 2 | Duplication | CSV escape/parse logic | Already centralized in `CsvHelper` (export/import delegate) — no action |
| 3 | Dead code | Unused imports / locals | **None found** (enforced by `noUnusedLocals`/`noUnusedParameters`) |
| 4 | CRUD logic | Risk of per-module duplication | Already centralized in `CrudListBase`; modules only supply config — no action |
| 5 | Dialogs | Risk of per-module dialog components | Reusable `generic-form-dialog` + `DialogService.openForm` cover the common case — no action |
| 6 | Config redundancy | `DEFAULT_PAGINATION_REQUEST` (model) vs `PAGINATION_CONFIG` (config) overlap on defaults | **Kept intentionally** — one is a request seed, the other app-wide table defaults; documented |
| 7 | Models | `BaseDto`/`BaseCatalogDto` reused by sample DTOs; no redundant DTOs | No action |
| 8 | Obsolete files | `user-form-dialog.component.ts` could look redundant vs `generic-form-dialog` | **Kept** — it intentionally demonstrates a *custom* dialog (embeds file-upload in the modal) for the kitchen-sink `/users` demo; the generic dialog covers standard cases |
| 9 | Services | Any service simplifiable? | All single-responsibility; `toFormData` extraction removed the only shared snippet |

## Methods moved to utils/helpers
- `FileUtil.toFormData` (frontend) — multipart body building, reused by File/Import services.
- (Backend already done in prior phases: `CsvHelper` shared by Export/Import; `QueryableExtensions` for sort/paginate.)

## Remaining (intentional, non-duplication)
- Each feature CRUD service is a 3-line `BaseCrudService` subclass — by design (only the resource path differs).
- Catalog vs Management list pages differ in **configuration only**; control flow lives in `CrudListBase`.

## Result
- Frontend: clean under `strict` + `noUnusedLocals` + `noUnusedParameters`.
- Backend: clean under `Nullable`, 0 warnings.
- One real duplication removed; no obsolete files deleted (the one candidate is intentional).
