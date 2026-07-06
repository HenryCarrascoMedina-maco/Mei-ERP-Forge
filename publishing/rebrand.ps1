<#
.SYNOPSIS
  Rebrand the starter kit's user-facing identity with minimal impact. Dry-run by default.

.DESCRIPTION
  Updates the single branding source (frontend branding.ts appName), the pre-load browser title
  (index.html) and the backend assembly metadata (Directory.Build.props: Product/Company/Authors).

  It deliberately does NOT rename framework packages or namespaces (@erp-platform/*, ErpPlatform.*,
  ErpBackend.*). That optional deep rename is documented in docs/REBRANDING.md. Keeping those stable
  is what makes this a low-impact, non-disruptive rebrand.

.PARAMETER Name
  New product name (shown in the toolbar, the login screen and the browser tab). Required.

.PARAMETER Company
  New company/author for assembly metadata. Optional (defaults to -Name).

.PARAMETER Apply
  Write the changes. Without it the script only previews (dry-run).

.EXAMPLE
  ./publishing/rebrand.ps1 -Name "Acme ERP"
  ./publishing/rebrand.ps1 -Name "Acme ERP" -Company "Acme Inc." -Apply
#>
param(
    [Parameter(Mandatory = $true)][string]$Name,
    [string]$Company,
    [switch]$Apply
)

$ErrorActionPreference = 'Stop'
if (-not $Company) { $Company = $Name }
$root = Split-Path -Parent $PSScriptRoot

$edits = @(
    @{ File = 'frontend/erp-frontend/src/app/branding.ts';  Pattern = "appName: '.*?'";          Replacement = "appName: '$Name'" }
    @{ File = 'frontend/erp-frontend/src/index.html';       Pattern = '<title>.*?</title>';       Replacement = "<title>$Name</title>" }
    @{ File = 'backend/ErpBackend/Directory.Build.props';   Pattern = '<Product>.*?</Product>';    Replacement = "<Product>$Name</Product>" }
    @{ File = 'backend/ErpBackend/Directory.Build.props';   Pattern = '<Company>.*?</Company>';    Replacement = "<Company>$Company</Company>" }
    @{ File = 'backend/ErpBackend/Directory.Build.props';   Pattern = '<Authors>.*?</Authors>';    Replacement = "<Authors>$Company</Authors>" }
)

$mode = if ($Apply) { '' } else { '  [DRY RUN]' }
Write-Host ("Rebrand to '{0}' (company '{1}'){2}" -f $Name, $Company, $mode) -ForegroundColor Cyan

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
foreach ($e in $edits) {
    $path = Join-Path $root $e.File
    if (-not (Test-Path $path)) { Write-Warning "skip (not found): $($e.File)"; continue }
    $content = Get-Content -Raw -Path $path
    if ($content -notmatch $e.Pattern) { Write-Warning "no match in $($e.File): $($e.Pattern)"; continue }
    $new = [regex]::Replace($content, $e.Pattern, $e.Replacement)
    Write-Host ("  {0}  ->  {1}" -f $e.File, $e.Replacement)
    if ($Apply) { [System.IO.File]::WriteAllText($path, $new, $utf8NoBom) }
}

if (-not $Apply) {
    Write-Host "`nDry run. Re-run with -Apply to write the changes." -ForegroundColor Yellow
}
else {
    Write-Host "`nDone. Two manual assets remain (by design):" -ForegroundColor Green
    Write-Host "  - replace  frontend/erp-frontend/public/favicon.ico"
    Write-Host "  - adjust the theme accent in  frontend/erp-frontend/src/styles.scss"
    Write-Host "See docs/REBRANDING.md for the OPTIONAL package/namespace rename."
}
