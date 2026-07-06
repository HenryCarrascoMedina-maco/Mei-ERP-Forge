# MeiCarOrt ERP Forge — Commercial Installation Guide

**Product (commercial name):** MeiCarOrt ERP Forge — *ERP Platform Starter Kit*
**Status:** v1.0.0-rc (release candidate)

> **Naming note.** *MeiCarOrt ERP Forge* is the proposed commercial brand. The packages currently
> ship under technical placeholder names and **will not be renamed until the RC is finalized**:
>
> | Commercial concept | Technical package (current) |
> |--------------------|-----------------------------|
> | Frontend library + generator | `@erp-platform/core` (npm) |
> | Backend library | `ErpPlatform.CrossCutting` (NuGet) |
> | Backend generator CLI | `ErpPlatform.Cli` → `erpgen` (dotnet tool) |
> | Backend module templates | `ErpPlatform.Templates` (dotnet new) |
>
> When the brand is locked, these become `@meicarort/*` / `MeiCarOrt.ErpForge.*` (one-time rename).

This guide is for **customer teams** installing the product into their own projects. Developers
building a module step-by-step should also read the [Getting Started](GETTING-STARTED.md).

---

## 1. What is MeiCarOrt ERP Forge?

A commercial **ERP platform starter kit** for building enterprise ERP applications fast on
**Angular 19 + .NET 10**. You describe a module once in a small JSON manifest, and the platform
**generates a working frontend module and backend module** — consistent UI, REST contracts,
pagination, validation, permissions and error handling — so your team focuses on business logic
instead of plumbing.

- **Define once, generate full-stack:** `Customer.module.json` → Angular feature + ASP.NET Core module.
- **Reusable core, not copied folders:** versioned npm + NuGet packages you install and update.
- **Regeneration-safe:** generated files are owned by the tools; your custom code is never overwritten.

## 2. What the customer receives

| Artifact | Technical name | Purpose |
|----------|----------------|---------|
| **Frontend package (npm)** | `@erp-platform/core` | Angular component & service library + interceptors/guards/pipes/validators + bundled schematics (`ng add`, `ng generate :module`) + manifest schema/validator |
| **Backend package (NuGet)** | `ErpPlatform.CrossCutting` | ASP.NET Core cross-cutting layer: responses/pagination, error handling, JWT auth + permission authorization, logging/audit, helpers, file/export/import |
| **Backend generator (dotnet tool)** | `ErpPlatform.Cli` (`erpgen`) | Generates a backend module from the same manifest |
| **Backend templates (dotnet new)** | `ErpPlatform.Templates` | `dotnet new erp-module` skeleton for quick starts |
| **Documentation** | — | Getting Started, Developer Guide, Packaging, Manifest Contract, Commercial Demo |
| **Example modules** | — | `Customer.module.json` and `Product.module.json` (validated, golden references) |

## 3. Install from the private registry

You will receive a **private npm registry URL**, a **private NuGet feed URL**, and credentials.

**Frontend (npm scope `@erp-platform`)** — create/append `.npmrc` in your Angular project:
```ini
@erp-platform:registry=https://<your-private-npm-registry>/
//<your-private-npm-registry>/:_authToken=${ERP_NPM_TOKEN}
```
```bash
npm i @erp-platform/core
ng add @erp-platform/core        # wires HttpClient+interceptors, animations, provideErpPlatform({ apiUrl })
```

**Backend (NuGet)** — add the feed and the package:
```bash
dotnet nuget add source https://<your-private-nuget-feed>/index.json -n erp-platform
dotnet add package ErpPlatform.CrossCutting
```

**Generator (dotnet tool)** from the same feed:
```bash
dotnet tool install -g ErpPlatform.Cli --add-source https://<your-private-nuget-feed>/index.json
dotnet new install ErpPlatform.Templates      # optional: dotnet new erp-module skeletons
```

## 4. Configure credentials (securely)

- **Never commit tokens.** Use environment variables / a secret store and reference them.
- **npm:** put the token in an env var `ERP_NPM_TOKEN` and reference it from `.npmrc` as shown
  (`${ERP_NPM_TOKEN}`). For CI, inject the token as a masked secret.
- **NuGet:** prefer `dotnet nuget add source <url> -n erp-platform -u <user> -p <token>` (omit
  `--store-password-in-clear-text`; let the OS credential store hold it), or configure the feed
  credentials in CI via the feed provider's auth (PAT / API key as a masked secret).
- Rotate tokens per your security policy; access is per the license seat agreement.

### JWT signing key (backend)

The backend's JWT signing key is read from configuration (`Jwt:SecretKey`). **No real key is
committed**: production `appsettings.json` has no `Jwt` section, and the only value in the repo is a
self-describing dev placeholder in `appsettings.Development.json` that keeps the local demo zero-config.

Provide a real key without touching the repo — both override the placeholder:

```bash
# Option A — .NET User Secrets (local dev; stored outside the repo)
dotnet user-secrets set "Jwt:SecretKey" "<a strong 32+ char key>" --project ErpBackend.Api

# Option B — environment variable (servers / CI / containers)
setx Jwt__SecretKey "<a strong 32+ char key>"      # Windows
export Jwt__SecretKey="<a strong 32+ char key>"    # Linux/macOS
```

> In production, **always** set a strong key via env var / secret store / Key Vault, and replace the
> demo users in `Samples/Auth/AuthController.cs` with your real identity provider.

## 5. Generate your first module

```bash
# Frontend
ng generate @erp-platform/core:module --manifest=Customer.module.json

# Backend (same manifest)
erpgen module --manifest Customer.module.json --project ./YourApi --namespace YourApi
```
Then `dotnet build` + `ng build`, run both, and the module is live (CRUD, pagination, filters,
validation, permissions). See the [Getting Started](GETTING-STARTED.md) §9 for the full walkthrough.

## 6. Update versions

The product follows **SemVer**; within a major version, updates are backward-compatible (additive).
```bash
npm i @erp-platform/core@latest             # frontend (or pin: @1.1.0)
dotnet add package ErpPlatform.CrossCutting --version 1.1.0
dotnet tool update -g ErpPlatform.Cli
dotnet new update                            # template pack
```
After updating, **re-run the generators** to refresh `*.generated.*` / `*.Generated.cs` (your
editable files and business logic are preserved). Review the CHANGELOG before adopting a new version.

## 7. Report errors / request support

- **Support channel:** `<support@your-company>` (or the customer portal URL provided with your license).
- **Include in every report:** product version (npm + NuGet + CLI), the `*.module.json` used, the exact
  command, the full console output / stack trace, and a minimal reproduction if possible.
- **Severity & response targets** are defined in your commercial agreement (SLA). Security issues:
  use the dedicated security contact, do not post publicly.

## 8. What the license includes

> Indicative summary — the binding terms are in your commercial license agreement.
- Use of the packages (`@erp-platform/core`, `ErpPlatform.CrossCutting`, `ErpPlatform.Cli`,
  `ErpPlatform.Templates`) to build and ship **your own** ERP applications.
- Unlimited module generation for your licensed projects/seats.
- Version updates within your licensed major line and the agreed support tier.
- The documentation and example modules.

## 9. What is NOT included

- Redistribution, resale, or sublicensing of the packages or their source as a competing product.
- Bespoke/custom module development, data modeling, or business logic (consulting, sold separately).
- Hosting, infrastructure, database setup, and EF Core implementation (the platform ships an
  in-memory reference repository; production persistence is your implementation).
- Roadmap capabilities not yet released: **multi-tenancy, white-label theming, licensing server,
  Excel/PDF export, CI/CD pipelines, automated test suites** (see the roadmap).
- Third-party licenses (Angular, Angular Material, .NET) — governed by their own terms.

## 10. Customer installation checklist

- [ ] Prerequisites installed: Node 20+, npm 10+, .NET 10 SDK, Angular CLI 19.
- [ ] Private **npm** registry configured in `.npmrc` (scope `@erp-platform`) with a token via env var.
- [ ] Private **NuGet** feed added (`dotnet nuget add source …`) with credentials in the OS/CI store.
- [ ] `npm i @erp-platform/core` succeeds; `ng add @erp-platform/core` wires the app config.
- [ ] `dotnet add package ErpPlatform.CrossCutting` succeeds; `Program.cs` wired (`AddErpCrossCutting`, `AddErpApiValidation`, JWT/authorization, `UseErpCrossCutting`).
- [ ] `dotnet tool install -g ErpPlatform.Cli` (and optionally `dotnet new install ErpPlatform.Templates`).
- [ ] Manifest validates; **first module generated** (frontend + backend) and both build clean.
- [ ] App runs; the generated module's CRUD works against the API (auth + permissions verified).
- [ ] Support channel and license terms acknowledged by the team.

---

*Brand: MeiCarOrt ERP Forge. Technical packages remain `@erp-platform/*` / `ErpPlatform.*` until the
v1.0.0 rename. See [Packaging](PACKAGING.md) and [Getting Started](GETTING-STARTED.md).*
