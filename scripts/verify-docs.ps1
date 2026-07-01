<#
.SYNOPSIS
  Performs lightweight documentation verification for AI engineering docs.

.DESCRIPTION
  Checks for required AI documentation files and common fake-verification wording in docs/ai.
  This is not a replacement for manual review, but it provides executable evidence for documentation-only phases.

  The fake-verification wording check ignores lines that intentionally document prohibited wording, such as anti-fake-verification rules.
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

$allowedContextPatterns = @(
    'anti-fake',
    'fake-verification',
    'forbidden',
    'do not',
    'must not',
    'invalid evidence',
    'vague wording',
    'prohibited wording'
)

$hits = New-Object System.Collections.Generic.List[object]
Get-ChildItem $docsPath -Recurse -File -Include *.md | ForEach-Object {
    $path = $_.FullName
    $relativePath = Resolve-Path $path -Relative
    $lines = Get-Content $path

    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        foreach ($pattern in $forbiddenPatterns) {
            if ($line -match [regex]::Escape($pattern)) {
                $lowerLine = $line.ToLowerInvariant()
                $isAllowedContext = $false
                foreach ($allowed in $allowedContextPatterns) {
                    if ($lowerLine.Contains($allowed)) {
                        $isAllowedContext = $true
                        break
                    }
                }

                if (-not $isAllowedContext) {
                    $hits.Add([pscustomobject]@{
                        File = $relativePath
                        Line = $index + 1
                        Pattern = $pattern
                    }) | Out-Null
                }
            }
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
Write-Host 'Evidence: required AI docs exist and common fake-verification wording was not found outside allowed anti-fake-verification context under docs/ai.'
exit 0
