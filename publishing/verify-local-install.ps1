<#
.SYNOPSIS  Simulates the CLIENT install experience offline, using a LOCAL folder feed (no public/
           private registry, no secrets). Proves every install command in the Commercial Install Guide
           AND the full backend flow: add CrossCutting + Persistence, install the CLI, generate an EF
           module from a manifest, and build the generated project.
.DESCRIPTION
  1. Packs all artifacts (unless -SkipPack) into dist-packages/.
  2. In a throwaway temp folder, using dist-packages/ as a local --source:
       [1] dotnet new web + add ErpPlatform.CrossCutting + ErpPlatform.Persistence (+ AppDbContext) + build
       [2] dotnet tool install ErpPlatform.Cli  -> erpgen --help
       [3] erpgen module --store ef             -> build the generated project (the key gate)
       [4] dotnet new install ErpPlatform.Templates
       [5] dotnet new erp-module               (template smoke test)
       [6] npm install <@erp-platform/core tarball>
     Also asserts the restored ErpPlatform.* versions are the expected one and that NO rc.0 leaks in.
  3. Cleans up the temp folder and the installed template pack.
.NOTES  Read-only against your real environment except for a temp dir it removes. Every step asserts
        its exit code, so a failure aborts loudly (never passes silently).
#>
param(
    [string]$PackagesDir = (Join-Path $PSScriptRoot "..\dist-packages"),
    [string]$ExpectedVersion = "1.0.0-rc.2",
    [switch]$SkipPack
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..")

if (-not $SkipPack) { & (Join-Path $PSScriptRoot "pack-all.ps1") -OutDir $PackagesDir }
$PackagesDir = (Resolve-Path $PackagesDir).Path

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("erp-verify-" + [System.Guid]::NewGuid().ToString("N").Substring(0,8))
New-Item -ItemType Directory -Force -Path $work | Out-Null
$toolDir = Join-Path $work "tools"

function Assert-LastExit([string]$step) {
    if ($LASTEXITCODE -ne 0) { throw "FAILED: $step (exit code $LASTEXITCODE)" }
}
function Cleanup {
    try { dotnet new uninstall ErpPlatform.Templates 2>$null | Out-Null } catch {}
    if (Test-Path $work) { Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue }
}

# --- Files written into the generated client project (literal here-strings) ---
$appDbContext = @'
using ErpBackend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClientApi;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : ErpDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ErpBackend.Identity.Configuration.IdentityModelBuilderExtensions.ApplyErpIdentity(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
'@

$programCs = @'
using ClientApi;
using ErpBackend.CrossCutting.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddErpCrossCutting(builder.Configuration);
builder.Services.AddErpApiValidation();
builder.Services.AddErpPersistence<AppDbContext>(builder.Configuration);

// erp-generated:modules

builder.Services.AddErpJwtAuthentication(builder.Configuration);
builder.Services.AddErpAuthorization();
builder.Services.AddErpIdentity(builder.Configuration);

var app = builder.Build();
app.UseErpCrossCutting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
'@

$manifest = @'
{
  "schemaVersion": "1.0",
  "module": { "name": "Invoice", "key": "invoices", "displayName": "Invoice" },
  "entity": {
    "base": "catalog",
    "idType": "guid",
    "fields": [
      { "name": "reference", "type": "text", "label": "Reference", "required": true, "validation": { "maxLength": 40 } },
      { "name": "customerName", "type": "text", "label": "Customer", "required": true },
      { "name": "amount", "type": "currency", "label": "Amount", "required": true, "validation": { "min": 0 } },
      { "name": "isActive", "type": "boolean", "label": "Status", "default": true }
    ]
  },
  "permissions": { "prefix": "invoices" },
  "list": { "search": true },
  "api": { "basePath": "billing", "resource": "invoices" }
}
'@

try {
    Write-Host "==> Local folder feed: $PackagesDir (expecting version $ExpectedVersion)" -ForegroundColor Cyan

    Write-Host "`n==> [1/6] dotnet new web + add CrossCutting + Persistence + Identity" -ForegroundColor Cyan
    Push-Location $work
    dotnet new web -n ClientApi --no-restore | Out-Null
    dotnet add ClientApi/ClientApi.csproj package ErpPlatform.CrossCutting --source $PackagesDir --prerelease
    Assert-LastExit "dotnet add package ErpPlatform.CrossCutting"
    dotnet add ClientApi/ClientApi.csproj package ErpPlatform.Persistence --source $PackagesDir --prerelease
    Assert-LastExit "dotnet add package ErpPlatform.Persistence"
    dotnet add ClientApi/ClientApi.csproj package ErpPlatform.Identity --source $PackagesDir --prerelease
    Assert-LastExit "dotnet add package ErpPlatform.Identity"
    Set-Content -Path "ClientApi/AppDbContext.cs" -Value $appDbContext -Encoding utf8
    Set-Content -Path "ClientApi/Program.cs" -Value $programCs -Encoding utf8
    dotnet build ClientApi/ClientApi.csproj -c Release --nologo
    Assert-LastExit "build client app (CrossCutting + Persistence + Identity)"
    Pop-Location

    Write-Host "`n==> [2/6] dotnet tool install ErpPlatform.Cli (erpgen)" -ForegroundColor Cyan
    dotnet tool install ErpPlatform.Cli --tool-path $toolDir --add-source $PackagesDir --prerelease
    Assert-LastExit "dotnet tool install ErpPlatform.Cli"
    & (Join-Path $toolDir "erpgen") --help | Out-Null
    Assert-LastExit "erpgen --help"

    Write-Host "`n==> [3/6] erpgen module --store ef + build generated project (KEY GATE)" -ForegroundColor Cyan
    Set-Content -Path "$work/Invoice.module.json" -Value $manifest -Encoding utf8
    & (Join-Path $toolDir "erpgen") module --manifest "$work/Invoice.module.json" --project "$work/ClientApi" --namespace ClientApi --store ef
    Assert-LastExit "erpgen module --store ef"
    $genFiles = Get-ChildItem "$work/ClientApi/Modules/Invoice" -File | Select-Object -ExpandProperty Name
    Write-Host "    generated: $($genFiles -join ', ')"
    if ($genFiles -notcontains "InvoiceConfiguration.Generated.cs" -or $genFiles -notcontains "InvoiceEfRepository.Generated.cs") {
        throw "EF artifacts (Configuration/EfRepository) not generated"
    }
    if ($genFiles -contains "InvoiceInMemoryRepository.cs") { throw "Unexpected InMemory repository in --store ef output" }
    dotnet build "$work/ClientApi/ClientApi.csproj" -c Release --nologo
    Assert-LastExit "build generated EF module"

    Write-Host "`n==> [verify] restored ErpPlatform.* versions = $ExpectedVersion, no rc.0 leak" -ForegroundColor Cyan
    $assets = Get-Content "$work/ClientApi/obj/project.assets.json" -Raw
    foreach ($pkg in @("ErpPlatform.CrossCutting", "ErpPlatform.Persistence", "ErpPlatform.Identity")) {
        if ($assets -notmatch [regex]::Escape("$pkg/$ExpectedVersion")) { throw "$pkg/$ExpectedVersion not found in restore graph" }
    }
    if ($assets -match "ErpPlatform\.\w+/1\.0\.0-rc\.[01]\b") { throw "a stale rc.0/rc.1 reference leaked into the restore graph" }
    Write-Host "    OK: CrossCutting + Persistence + Identity resolved at $ExpectedVersion; no stale rc present" -ForegroundColor Green

    Write-Host "`n==> [4/6] dotnet new install ErpPlatform.Templates" -ForegroundColor Cyan
    dotnet new install (Get-ChildItem $PackagesDir -Filter "ErpPlatform.Templates.*.nupkg" | Select-Object -First 1).FullName
    Assert-LastExit "dotnet new install ErpPlatform.Templates"

    Write-Host "`n==> [5/6] dotnet new erp-module (template smoke test)" -ForegroundColor Cyan
    $modDir = Join-Path $work "ModuleTest"
    New-Item -ItemType Directory -Force -Path $modDir | Out-Null
    Push-Location $modDir
    dotnet new erp-module --resource Invoice --prefix sales
    Assert-LastExit "dotnet new erp-module"
    Pop-Location

    Write-Host "`n==> [6/6] npm install @erp-platform/core (tarball)" -ForegroundColor Cyan
    $tgz = Get-ChildItem $PackagesDir -Filter "erp-platform-core-*.tgz" | Select-Object -First 1
    if (-not $tgz) { throw "npm tarball not found in $PackagesDir" }
    if ($tgz.Name -notmatch [regex]::Escape($ExpectedVersion)) { throw "npm tarball is not $ExpectedVersion ($($tgz.Name))" }
    $npmDir = Join-Path $work "npm-client"
    New-Item -ItemType Directory -Force -Path $npmDir | Out-Null
    Push-Location $npmDir
    npm init -y | Out-Null
    npm install $tgz.FullName --no-save
    Assert-LastExit "npm install @erp-platform/core"
    $pkgJson = Join-Path $npmDir "node_modules\@erp-platform\core\package.json"
    if (-not (Test-Path $pkgJson)) { throw "@erp-platform/core not found in node_modules" }
    $coreVersion = (Get-Content $pkgJson -Raw | ConvertFrom-Json).version
    if ($coreVersion -ne $ExpectedVersion) { throw "@erp-platform/core is $coreVersion, expected $ExpectedVersion" }
    Write-Host "    @erp-platform/core@$coreVersion unpacked OK" -ForegroundColor Green
    Pop-Location

    Write-Host "`n==================================================" -ForegroundColor Green
    Write-Host " ALL CLIENT INSTALL STEPS PASSED ($ExpectedVersion, offline feed)" -ForegroundColor Green
    Write-Host "==================================================" -ForegroundColor Green
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    Write-Host "`n==> Cleanup" -ForegroundColor DarkGray
    Cleanup
}
