<#
.SYNOPSIS  Pushes the ErpPlatform.* NuGet packages to a PRIVATE feed.
.NOTES     Provide the feed URL and API key via environment variables (masked CI secrets):
             $env:ERP_NUGET_FEED  = "https://YOUR-PRIVATE-NUGET-FEED/index.json"
             $env:ERP_NUGET_KEY   = "<api-key or PAT>"
           Never commit secrets. Does NOT push to nuget.org.
#>
param(
    [string]$PackagesDir = (Join-Path $PSScriptRoot "..\dist-packages"),
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"
if (-not $env:ERP_NUGET_FEED) { throw "Set `$env:ERP_NUGET_FEED to your private feed URL." }
$key = if ($env:ERP_NUGET_KEY) { $env:ERP_NUGET_KEY } else { "" }

$nupkgs = Get-ChildItem (Resolve-Path $PackagesDir) -Filter "ErpPlatform.*.nupkg"
if (-not $nupkgs) { throw "No ErpPlatform.*.nupkg found in $PackagesDir. Run pack-all.ps1 first." }

foreach ($n in $nupkgs) {
    Write-Host "==> push $($n.Name) -> $env:ERP_NUGET_FEED" -ForegroundColor Cyan
    if ($WhatIf) { continue }
    dotnet nuget push $n.FullName --source $env:ERP_NUGET_FEED --api-key $key --skip-duplicate
}
