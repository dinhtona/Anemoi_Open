<#
.SYNOPSIS
  Performs lightweight documentation verification for AI engineering docs.

.DESCRIPTION
  Checks for required AI documentation files and common fake-verification wording in docs/ai.
  This is not a replacement for manual review, but it provides executable evidence for documentation-only phases.
#>

[CmdletBinding()]
param(
    [string]$DocsRoot = 'docs/ai'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$requiredFiles = @(
    'AGENTS.md',
    'docs/AI_ENGINEERING_HANDBOOK.md',
    'docs/README.md',
    'docs/ai/README.md',
    'docs/ai/core/verification-standard.md',
    'docs/ai/verification/README.md'
)

$missing = @()
foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    Write-Host 'Status: FAILED' -ForegroundColor Red
    Write-Host 'Missing required files:'
    $missing | ForEach-Object { Write-Host "- $_" }
    exit 1
}

$docsPath = Join-Path $repoRoot $DocsRoot
if (-not (Test-Path $docsPath)) {
    Write-Host 'Status: FAILED' -ForegroundColor Red
    Write-Host "Docs root not found: $DocsRoot"
    exit 1
}

$forbiddenPatterns = @(
    'should work',
    'looks good',
    'probably works',
    'verified without running',
    'tests passed previously',
    'assume pass'
)

$hits = New-Object System.Collections.Generic.List[object]
Get-ChildItem $docsPath -Recurse -File -Include *.md | ForEach-Object {
    $path = $_.FullName
    $content = Get-Content $path -Raw
    foreach ($pattern in $forbiddenPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            $hits.Add([pscustomobject]@{
                File = Resolve-Path $path -Relative
                Pattern = $pattern
            }) | Out-Null
        }
    }
}

if ($hits.Count -gt 0) {
    Write-Host 'Status: FAILED' -ForegroundColor Red
    Write-Host 'Potential fake-verification wording found:'
    $hits | Format-Table -AutoSize
    exit 1
}

Write-Host 'Status: PASS' -ForegroundColor Green
Write-Host 'Evidence: required AI docs exist and common fake-verification wording was not found under docs/ai.'
exit 0
