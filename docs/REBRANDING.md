# Rebranding Guide

Make the starter kit *yours* with the least possible change. The framework's internal package names
(`@erp-platform/*`, `ErpPlatform.*`, `ErpBackend.*`) are its stable identity — you consume them the
way you consume Angular or .NET, and you **don't** need to rename them to ship your product. Rebranding
is about the **user-facing identity** and your **environment configuration**.

---

## 1. Quick rebrand (user-facing name) — ~2 minutes

Everything the user sees comes from **one file**:
[`frontend/erp-frontend/src/app/branding.ts`](../frontend/erp-frontend/src/app/branding.ts)

```ts
export const BRANDING = {
  appName: 'ERP Template',          // toolbar, login screen, browser tab
  shortName: 'ERP',                 // compact contexts
  description: 'Reusable ERP starter kit',   // login subtitle
} as const;
```

Change those values and you're done — the toolbar, the login screen and the browser tab all update.

Or run the helper (dry-run by default; `-Apply` to write). It also updates the pre-load
`index.html` title and the backend assembly metadata:

```powershell
./publishing/rebrand.ps1 -Name "Acme ERP" -Company "Acme Inc."          # preview
./publishing/rebrand.ps1 -Name "Acme ERP" -Company "Acme Inc." -Apply   # write
```

Two assets remain manual (by design — they're binary/visual):
- **Favicon** — replace `frontend/erp-frontend/public/favicon.ico`.
- **Theme accent** — adjust the Angular Material theme in `frontend/erp-frontend/src/styles.scss`.

---

## 2. Environment configuration (per deployment)

These are configuration, not code — set them for your environments:

| What | Where |
|------|-------|
| API base URL | `frontend/erp-frontend/src/environments/environment*.ts` → `apiUrl` (passed to `provideErpPlatform`) |
| Initial admin (user/email) | `backend/.../ErpBackend.Api/appsettings*.json` → `Identity:DefaultAdmin` |
| Admin password | User-secrets / env var (never in source) → `Identity:DefaultAdmin:Password` |
| JWT signing key | User-secrets / env var → `Jwt:SecretKey` (32+ chars) |
| Allowed CORS origins | `appsettings*.json` → `Cors:AllowedOrigins` (empty = permissive dev fallback) |
| Your modules' permissions | `appsettings*.json` → `Identity:AdditionalAdminPermissions` (the admin role auto-reconciles on startup) |

---

## 3. Optional: rename the framework packages (advanced)

Only if you intend to **republish** the framework under your own npm scope / NuGet prefix. This is a
bounded find-replace; the assembly namespaces (`ErpBackend.*`) can stay as-is — renaming them is a
larger, unnecessary change for building on top of the kit.

- **npm scope** `@erp-platform` → `@yourscope`: update `projects/erp-platform/package.json` `name`,
  the ~93 `from '@erp-platform/core'` imports, the app `tsconfig` path mapping, and `ng-package.json`.
- **NuGet prefix** `ErpPlatform` → `YourPrefix`: update the `PackageId` in the packable `.csproj`
  files, `.config/dotnet-tools.json`, and `publishing/pack-all.ps1`.

> Do this on a branch and rely on the build + tests + `dotnet pack` / `ng build` to catch anything
> missed. If you're only *using* the kit (not reselling the framework), skip this section entirely.

---

## What you do NOT need to touch
- Generated modules keep working — regenerate your own with `erpgen` (see
  [Getting Started](GETTING-STARTED.md)) and delete the `Customer`/`Product` examples.
- The component library, services, generator and CI are framework internals — leave them.
