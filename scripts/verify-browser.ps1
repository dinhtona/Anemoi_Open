<#
.SYNOPSIS
  Provides a browser verification checklist and optional route reachability check.

.DESCRIPTION
  Browser validation should normally be executed through DevTools MCP or an equivalent browser automation tool.
  This helper prevents fake PASS by producing a required evidence checklist and optionally checking HTTP reachability for a route.

.PARAMETER AppBaseUrl
  Frontend base URL, for example http://localhost:3000.

.PARAMETER Route
  Route to check, for example /en/ess/leave.

.PARAMETER Persona
  Persona used for the browser test, such as Employee, Manager, HR, Recruiter, or Administrator.
#>

[CmdletBinding()]
param(
    [string]$AppBaseUrl,
    [string]$Route = '/',
    [string]$Persona = 'unspecified'
)

$ErrorActionPreference = 'Stop'

$checks = @(
    'Page loads without runtime exception',
    'No unexpected console errors',
    'No failed network requests for required data',
    'Main user flow succeeds',
    'Loading, empty, and error states are acceptable',
    'Permission visibility is correct for the persona',
    'Localization is correct for touched EN/VI text',
    'Data-changing flows verify the resulting API/database/workflow state when applicable'
)

Write-Host 'Browser verification checklist' -ForegroundColor Green
Write-Host "Persona: $Persona"
Write-Host "Route: $Route"
$checks | ForEach-Object { Write-Host "[ ] $_" }

if ([string]::IsNullOrWhiteSpace($AppBaseUrl)) {
    Write-Host "`nStatus: REVIEWED" -ForegroundColor Yellow
    Write-Host 'Reason: no -AppBaseUrl supplied. Checklist generated only; no browser/runtime verification was executed.'
    exit 0
}

$uri = $AppBaseUrl.TrimEnd('/') + '/' + $Route.TrimStart('/')
Write-Host "`nChecking route reachability: $uri" -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri $uri -Method Get -UseBasicParsing
    if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
        Write-Host 'Route responded successfully.' -ForegroundColor Green
        Write-Host 'Status: PARTIAL'
        Write-Host 'Reason: HTTP reachability passed, but DevTools MCP browser checks are still required for PASS.'
        exit 0
    }

    Write-Host "Status: FAILED" -ForegroundColor Red
    Write-Host "Reason: route returned HTTP $($response.StatusCode)."
    exit 1
}
catch {
    Write-Host 'Status: FAILED' -ForegroundColor Red
    Write-Host "Reason: route reachability failed: $($_.Exception.Message)"
    exit 1
}
