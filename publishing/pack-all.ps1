<#
.SYNOPSIS  Builds and packs all ERP Platform artifacts (npm tarball + NuGet packages) into dist-packages/.
.NOTES     No secrets involved. Run before publish-* or verify-local-install.
#>
param([string]$OutDir = (Join-Path $PSScriptRoot "..\dist-packages"))

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$fe = Join-Path $root "frontend\erp-frontend"
$be = Join-Path $root "backend\ErpBackend"
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path

Write-Host "==> Frontend: build library + schematics" -ForegroundColor Cyan
Push-Location $fe
npm run ng -- build erp-platform
npx tsc -p projects/erp-platform/schematics/tsconfig.json
node projects/erp-platform/schematics/copy-assets.mjs
Write-Host "==> npm pack @erp-platform/core"
npm pack (Join-Path $fe "dist\erp-platform") --pack-destination $OutDir
Pop-Location

Write-Host "==> Backend: dotnet pack (CrossCutting, Persistence, Identity, Cli, Templates)" -ForegroundColor Cyan
dotnet pack (Join-Path $be "ErpBackend.CrossCutting") -c Release -o $OutDir --nologo
dotnet pack (Join-Path $be "ErpBackend.Persistence") -c Release -o $OutDir --nologo
dotnet pack (Join-Path $be "ErpBackend.Identity") -c Release -o $OutDir --nologo
dotnet pack (Join-Path $be "tools\ErpPlatform.Cli") -c Release -o $OutDir --nologo
dotnet pack (Join-Path $be "packaging\ErpPlatform.Templates") -c Release -o $OutDir --nologo

Write-Host "`n==> Packaged artifacts in $OutDir" -ForegroundColor Green
Get-ChildItem $OutDir | Select-Object Name, Length | Format-Table -AutoSize
