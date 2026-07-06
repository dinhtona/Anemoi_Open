<#
.SYNOPSIS
  Runs the default Anemoi verification pipeline for AI engineering work.

.DESCRIPTION
  This script is intentionally conservative. It runs checks that can be executed from a clean checkout and reports honest status codes.
  It does not claim browser, API, workflow, database, or GitHub Actions verification unless the matching opt-in flags are used.

.PARAMETER SkipBackend
  Skip dotnet restore/build/test checks.

.PARAMETER SkipFrontend
  Skip cody-web-app install/build/lint checks.

.PARAMETER IncludeBrowser
  Run scripts/verify-browser.ps1 after frontend checks.

.PARAMETER IncludeWorkflow
  Run scripts/verify-workflow.ps1 after backend checks.

.PARAMETER IncludeArchitectureGuard
  Run scripts/architecture-guard.ps1.

.EXAMPLE
  pwsh ./scripts/verify.ps1

.EXAMPLE
  pwsh ./scripts/verify.ps1 -IncludeArchitectureGuard -IncludeWorkflow
#>

[CmdletBinding()]
param(
    [switch]$SkipBackend,
    [switch]$SkipFrontend,
    [switch]$IncludeBrowser,
    [switch]$IncludeWorkflow,
    [switch]$IncludeArchitectureGuard,
    [switch]$NoRestore
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$results = New-Object System.Collections.Generic.List[object]

function Add-Result {
    param(
        [string]$Name,
        [string]$Status,
        [string]$Evidence
    )

    $results.Add([pscustomobject]@{
        Name = $Name
        Status = $Status
        Evidence = $Evidence
    }) | Out-Null
}

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$Command
    )

    Write-Host "`n==> $Name" -ForegroundColor Cyan
    try {
        & $Command
        Add-Result -Name $Name -Status 'PASS' -Evidence 'Command completed with exit code 0.'
    }
    catch {
        Add-Result -Name $Name -Status 'FAILED' -Evidence $_.Exception.Message
        throw
    }
}

Write-Host 'Anemoi verification pipeline' -ForegroundColor Green
Write-Host "Repository: $repoRoot"

if (-not $SkipBackend) {
    if (-not $NoRestore) {
        Invoke-Step 'dotnet restore' { dotnet restore ./Anemoi.sln }
    }

    Invoke-Step 'dotnet build' { dotnet build ./Anemoi.sln --no-restore }
    Invoke-Step 'dotnet test' { dotnet test ./Anemoi.sln --no-build --verbosity normal }
}
else {
    Add-Result -Name 'Backend verification' -Status 'N/A' -Evidence 'Skipped by -SkipBackend.'
}

$frontendPath = Join-Path $repoRoot 'cody-web-app'
if (-not $SkipFrontend) {
    if (Test-Path $frontendPath) {
        Push-Location $frontendPath
        try {
            if (Test-Path './package-lock.json') {
                Invoke-Step 'frontend npm ci' { npm ci }
            }
            else {
                Invoke-Step 'frontend npm install' { npm install }
            }

            Invoke-Step 'frontend build' { npm run build }

            $packageJson = Get-Content './package.json' -Raw
            if ($packageJson -match '"lint"\s*:') {
                Invoke-Step 'frontend lint' { npm run lint }
            }
            else {
                Add-Result -Name 'frontend lint' -Status 'N/A' -Evidence 'No lint script found in cody-web-app/package.json.'
            }
        }
        finally {
            Pop-Location
        }
    }
    else {
        Add-Result -Name 'Frontend verification' -Status 'N/A' -Evidence 'cody-web-app directory not found.'
    }
}
else {
    Add-Result -Name 'Frontend verification' -Status 'N/A' -Evidence 'Skipped by -SkipFrontend.'
}

if ($IncludeArchitectureGuard) {
    Invoke-Step 'architecture guard' { pwsh ./scripts/architecture-guard.ps1 }
}
else {
    Add-Result -Name 'architecture guard' -Status 'N/A' -Evidence 'Not requested. Use -IncludeArchitectureGuard.'
}

if ($IncludeWorkflow) {
    Invoke-Step 'workflow verification helper' { pwsh ./scripts/verify-workflow.ps1 }
}
else {
    Add-Result -Name 'workflow verification helper' -Status 'N/A' -Evidence 'Not requested. Use -IncludeWorkflow.'
}

if ($IncludeBrowser) {
    Invoke-Step 'browser verification helper' { pwsh ./scripts/verify-browser.ps1 }
}
else {
    Add-Result -Name 'browser verification helper' -Status 'N/A' -Evidence 'Not requested. Use -IncludeBrowser.'
}

Write-Host "`nVerification summary" -ForegroundColor Green
$results | Format-Table -AutoSize

$failed = $results | Where-Object { $_.Status -eq 'FAILED' }
if ($failed.Count -gt 0) {
    exit 1
}

exit 0
