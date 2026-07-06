# Publishing Guide — Private Registries (npm + NuGet)

> Status: **v1.0.0-rc.0** · This guide covers publishing the ERP Platform artifacts to **private**
> registries and verifying the **client install experience** offline. It does **not** publish to any
> public registry (npm public / nuget.org). No tokens or secrets are committed to the repository.

## 1. Artifacts published

| Artifact | Type | Package ID | Client command |
|----------|------|-----------|----------------|
| Frontend library + schematics | npm (scoped) | `@erp-platform/core` | `npm install @erp-platform/core` → `ng add @erp-platform/core` |
| Backend cross-cutting library | NuGet | `ErpPlatform.CrossCutting` | `dotnet add package ErpPlatform.CrossCutting` |
| Backend generator CLI | NuGet (dotnet tool) | `ErpPlatform.Cli` (`erpgen`) | `dotnet tool install ErpPlatform.Cli` |
| Backend module template pack | NuGet (template) | `ErpPlatform.Templates` | `dotnet new install ErpPlatform.Templates` |

All artifacts share version **1.0.0-rc.0** (pre-release / `--prerelease` semantics).

## 2. Secrets policy

- **Never commit** `.npmrc`, `nuget.config`, tokens, API keys, or `.env` files. These are listed in
  [`.gitignore`](../.gitignore).
- Commit only the **examples**: [`.npmrc.example`](../.npmrc.example) and
  [`nuget.config.example`](../nuget.config.example).
- Provide credentials at runtime via **environment variables** (`ERP_NPM_TOKEN`, `ERP_NUGET_FEED`,
  `ERP_NUGET_KEY`) or your CI's masked-secrets store.

## 3. One-time setup (per developer / CI)

### npm
```powershell
Copy-Item .npmrc.example .npmrc          # then edit the registry URL
$env:ERP_NPM_TOKEN = "<your-private-registry-token>"   # never commit
```
Edit `.npmrc` so the `@erp-platform` scope points at your private registry.

### NuGet
```powershell
Copy-Item nuget.config.example nuget.config   # then edit the feed URL
# Store credentials encrypted (not in the file):
dotnet nuget add source "https://YOUR-PRIVATE-NUGET-FEED/index.json" `
  --name erp-platform --username <user> --password <token> --store-password-in-clear-text:$false
```

## 4. Pack

Builds the Angular library + schematics and runs `dotnet pack` for the three NuGet packages,
emitting everything to `dist-packages/`:

```powershell
./publishing/pack-all.ps1
```

Output (example):
```
@erp-platform-core-1.0.0-rc.0.tgz
ErpPlatform.CrossCutting.1.0.0-rc.0.nupkg
ErpPlatform.Cli.1.0.0-rc.0.nupkg
ErpPlatform.Templates.1.0.0-rc.0.nupkg
```

## 5. Publish to private registries

```powershell
# npm  — requires .npmrc + ERP_NPM_TOKEN
./publishing/publish-npm.ps1 -DryRun     # validate first
./publishing/publish-npm.ps1

# NuGet — requires ERP_NUGET_FEED (+ ERP_NUGET_KEY)
$env:ERP_NUGET_FEED = "https://YOUR-PRIVATE-NUGET-FEED/index.json"
$env:ERP_NUGET_KEY  = "<api-key>"
./publishing/publish-nuget.ps1 -WhatIf   # list what would push
./publishing/publish-nuget.ps1
```

`publish-npm.ps1` uses `--access restricted` (private). `publish-nuget.ps1` uses `--skip-duplicate`
so re-runs are safe. Neither targets a public registry.

## 6. Verify the client experience (offline, no registry, no secrets)

Before standing up a real private feed, you can prove every client install command works using a
**local folder feed** — exactly the bytes a registry would serve:

```powershell
./publishing/verify-local-install.ps1
```

This packs all artifacts, registers `dist-packages/` as a temporary NuGet source, then in a throwaway
temp folder runs and asserts:

1. `dotnet add package ErpPlatform.CrossCutting` + build
2. `dotnet tool install ErpPlatform.Cli` → `erpgen --help`
3. `dotnet new install ErpPlatform.Templates`
4. `dotnet new erp-module` (template smoke test)
5. `npm install @erp-platform/core` (from the packed `.tgz`)

It cleans up the temp folder, the temporary NuGet source, and the installed template pack on exit.

## 7. CI integration (sketch)

```yaml
# Pseudocode — adapt to GitHub Actions / Azure Pipelines
- run: pwsh ./publishing/pack-all.ps1
- run: pwsh ./publishing/verify-local-install.ps1 -SkipPack   # gate: client flow must pass
- run: pwsh ./publishing/publish-npm.ps1
  env: { ERP_NPM_TOKEN: ${{ secrets.ERP_NPM_TOKEN }} }
- run: pwsh ./publishing/publish-nuget.ps1
  env: { ERP_NUGET_FEED: ${{ secrets.ERP_NUGET_FEED }}, ERP_NUGET_KEY: ${{ secrets.ERP_NUGET_KEY }} }
```

## 8. Versioning

- npm version lives in `projects/erp-platform/package.json`.
- NuGet version lives in `backend/ErpBackend/Directory.Build.props` (`<Version>`).
- Keep them in lockstep. Current: **1.0.0-rc.0**. Bump both together for the next release.

## See also
- [Commercial Install Guide](./COMMERCIAL-INSTALL-GUIDE.md) — the client-facing install walkthrough.
- [Packaging](./PACKAGING.md) — how the packages are structured.
- [Getting Started](./GETTING-STARTED.md) — end-to-end first module.
