<#
.SYNOPSIS
  Runs Anemoi architecture guard checks.

.DESCRIPTION
  The guard combines a small PowerShell regex pre-check with the Roslyn-based console analyzer under tools/Anemoi.ArchitectureGuard.
  It is designed to catch AI-generated architecture violations before code review.

.PARAMETER SkipBuild
  Skip building the Roslyn analyzer project before running it.

.PARAMETER Path
  Repository-relative path to scan. Defaults to the repository root.
#>

[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [string]$Path = '.'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$scanRoot = Resolve-Path $Path
$violations = New-Object System.Collections.Generic.List[object]

function Add-Violation {
    param(
        [string]$Rule,
        [string]$File,
        [string]$Message
    )

    $violations.Add([pscustomobject]@{
        Rule = $Rule
        File = $File
        Message = $Message
    }) | Out-Null
}

function Test-IsExcludedPath {
    param([string]$FullName)

    $normalized = $FullName.Replace('\', '/')
    return $normalized.Contains('/bin/') `
        -or $normalized.Contains('/obj/') `
        -or $normalized.Contains('/.git/') `
        -or $normalized.Contains('/node_modules/') `
        -or $normalized.Contains('/Migrations/')
}

Write-Host 'Architecture Guard - PowerShell regex pre-check' -ForegroundColor Green
Write-Host "Scan root: $scanRoot"

Get-ChildItem $scanRoot -Recurse -File -Include *.cs,*.tsx,*.ts | Where-Object {
    -not (Test-IsExcludedPath -FullName $_.FullName)
} | ForEach-Object {
    $relative = Resolve-Path $_.FullName -Relative
    $content = Get-Content $_.FullName -Raw

    if ($_.Extension -eq '.cs') {
        if ($relative -match 'Controller\.cs$' -and $content -match 'DbContext') {
            Add-Violation -Rule 'ANEMOI001' -File $relative -Message 'Controller appears to reference DbContext. Controllers must delegate to CQRS handlers.'
        }

        if ($content -match 'Guid\.NewGuid\s*\(' -and $relative -notmatch 'Test|Tests|Migration|Migrations') {
            Add-Violation -Rule 'ANEMOI002' -File $relative -Message 'Production code appears to use Guid.NewGuid(). Use IdGenerator.NextGuid() where project standards require generated GUIDs.'
        }

        if ($content -match 'using\s+AutoMapper\s*;' -or $content -match 'IMapper\b') {
            Add-Violation -Rule 'ANEMOI003' -File $relative -Message 'AutoMapper usage detected. Use Mapperly according to project rules.'
        }

        if ($content -match '\[Authorize\s*\([^\)]*Roles\s*=') {
            Add-Violation -Rule 'ANEMOI004' -File $relative -Message 'Role-name-based authorization detected. Use permission constants and [HasPermission].'
        }
    }

    if ($_.Extension -in @('.ts', '.tsx')) {
        if ($content -match 'user\.roles' -or $content -match '\.roles\.includes') {
            Add-Violation -Rule 'ANEMOI005' -File $relative -Message 'Frontend role-based authorization pattern detected. Use user.permissions for authorization.'
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "`nPowerShell pre-check violations:" -ForegroundColor Red
    $violations | Format-Table -AutoSize
    exit 1
}

Write-Host 'PowerShell regex pre-check passed.' -ForegroundColor Green

$guardProject = Join-Path $repoRoot 'tools/Anemoi.ArchitectureGuard/Anemoi.ArchitectureGuard.csproj'
if (-not (Test-Path $guardProject)) {
    Write-Host 'Status: PARTIAL' -ForegroundColor Yellow
    Write-Host 'Reason: Roslyn guard project not found. PowerShell regex pre-check passed only.'
    exit 0
}

if (-not $SkipBuild) {
    Write-Host "`nBuilding Roslyn architecture guard" -ForegroundColor Cyan
    dotnet build $guardProject
}

Write-Host "`nRunning Roslyn architecture guard" -ForegroundColor Cyan
dotnet run --project $guardProject -- --path $scanRoot
