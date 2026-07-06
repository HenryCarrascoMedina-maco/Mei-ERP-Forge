<#
.SYNOPSIS  Publishes @erp-platform/core to the PRIVATE npm registry configured in .npmrc.
.NOTES     Requires a .npmrc (copy from .npmrc.example) and ERP_NPM_TOKEN in the environment.
           Never commit the token. Does NOT publish to the public npm registry.
#>
param([switch]$DryRun)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$pkg = Join-Path $root "frontend\erp-frontend\dist\erp-platform"

if (-not (Test-Path (Join-Path $pkg "package.json"))) {
    throw "Built package not found at $pkg. Run pack-all.ps1 first."
}
if (-not $env:ERP_NPM_TOKEN) {
    Write-Warning "ERP_NPM_TOKEN is not set. Set it (and a .npmrc from .npmrc.example) before publishing."
}

$npmArgs = @("publish", $pkg, "--access", "restricted")
if ($DryRun) { $npmArgs += "--dry-run" }
Write-Host "npm $($npmArgs -join ' ')" -ForegroundColor Cyan
npm @npmArgs
