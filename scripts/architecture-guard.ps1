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

# Shared, documented exemptions. tools/Anemoi.ArchitectureGuard reads the same file,
# so this regex pre-check and the Roslyn pass cannot drift apart.
$exemptionsPath = Join-Path $repoRoot 'scripts/architecture-guard-exemptions.json'
$guidExemptions = @()
$excludedScanPrefixes = @()
if (Test-Path $exemptionsPath) {
    $exemptions = Get-Content $exemptionsPath -Raw | ConvertFrom-Json
    if ($null -ne $exemptions.guidNewGuidExemptions) {
        $guidExemptions = @($exemptions.guidNewGuidExemptions |
            ForEach-Object { ($_.path -replace '\\', '/').Trim().TrimStart('.', '/').TrimEnd('/') } |
            Where-Object { $_ })
    }
    if ($null -ne $exemptions.excludedScanPrefixes) {
        $excludedScanPrefixes = @($exemptions.excludedScanPrefixes |
            ForEach-Object { (($_.ToString() -replace '\\', '/').Trim().TrimStart('.', '/').TrimEnd('/') + '/') } |
            Where-Object { $_ -ne '/' })
    }
    Write-Host "Exemptions: $($guidExemptions.Count) Guid.NewGuid() file(s), $($excludedScanPrefixes.Count) excluded scan prefix(es)"
} else {
    Write-Host "Exemptions: none (missing $exemptionsPath)" -ForegroundColor Yellow
}

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
    if ($normalized.Contains('/bin/') `
        -or $normalized.Contains('/obj/') `
        -or $normalized.Contains('/.git/') `
        -or $normalized.Contains('/node_modules/') `
        -or $normalized.Contains('/Migrations/')) {
        return $true
    }

    $relative = [System.IO.Path]::GetRelativePath($repoRoot.ProviderPath, $FullName).Replace('\', '/')
    foreach ($prefix in $excludedScanPrefixes) {
        if ($relative.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

Write-Host 'Architecture Guard - PowerShell regex pre-check' -ForegroundColor Green
Write-Host "Scan root: $scanRoot"

Get-ChildItem $scanRoot -Recurse -File -Include *.cs,*.tsx,*.ts | Where-Object {
    -not (Test-IsExcludedPath -FullName $_.FullName)
} | ForEach-Object {
    $relative = [System.IO.Path]::GetRelativePath($repoRoot.ProviderPath, $_.FullName).Replace('\', '/')
    $content = Get-Content $_.FullName -Raw

    if ($_.Extension -eq '.cs') {
        if ($relative -match 'Controller\.cs$' -and $content -match 'DbContext') {
            Add-Violation -Rule 'ANEMOI001' -File $relative -Message 'Controller appears to reference DbContext. Controllers must delegate to CQRS handlers.'
        }

        if ($content -match 'Guid\.NewGuid\s*\(' `
            -and $relative -notmatch 'Test|Tests|Migration|Migrations' `
            -and -not ($guidExemptions -contains $relative)) {
            Add-Violation -Rule 'ANEMOI002' -File $relative -Message 'Production code appears to use Guid.NewGuid(). Use IdGenerator.NextGuid() where project standards require generated GUIDs.'
        }

        if ($content -match 'using\s+AutoMapper\s*;' -or $content -match 'IMapper\b') {
            Add-Violation -Rule 'ANEMOI003' -File $relative -Message 'AutoMapper usage detected. Use Mapperly according to project rules.'
        }

        # ADR-036: the reserved SystemRoles.Administrator check on trust-anchor
        # endpoints is a sanctioned exception. Any other Roles usage is flagged.
        $roleArguments = [regex]::Matches($content, '\[Authorize\s*\([^\)]*Roles\s*=\s*([^,\)\]\s]+)')
        $unexpectedRoleArguments = @($roleArguments | Where-Object { $_.Groups[1].Value -ne 'SystemRoles.Administrator' })
        if ($unexpectedRoleArguments.Count -gt 0) {
            Add-Violation -Rule 'ANEMOI004' -File $relative -Message 'Role-name-based authorization detected. Use permission constants and [HasPermission]. See ADR-036 for the SystemRoles.Administrator exception.'
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
