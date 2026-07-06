<#
.SYNOPSIS
  Prints and optionally validates workflow-engine verification requirements.

.DESCRIPTION
  This helper does not fake workflow PASS. By default it produces a concrete evidence checklist for definition-bound workflow testing.
  When API base URL and token are supplied, it performs lightweight health-style HTTP checks only. Product-specific submit/approve flows still require scenario data.

.PARAMETER ApiBaseUrl
  Base URL for the HR/Centralize API, for example http://localhost:5000.

.PARAMETER BearerToken
  JWT bearer token for authenticated API checks.

.PARAMETER EntityType
  Workflow entity type to verify, such as LeaveRequest or OvertimeRequest.

.PARAMETER DefinitionEndpoint
  Optional endpoint that returns workflow definitions. Defaults to /api/hr/workflow/definitions.
#>

[CmdletBinding()]
param(
    [string]$ApiBaseUrl,
    [string]$BearerToken,
    [string]$EntityType = 'LeaveRequest',
    [string]$DefinitionEndpoint = '/api/hr/workflow/definitions'
)

$ErrorActionPreference = 'Stop'

$required = @(
    'Active WorkflowDefinition exists for the entity type',
    'Submit/start creates a WorkflowInstance',
    'Pending approval is visible only to the responsible approver',
    'Approve path uses workflow command/API, not legacy entity endpoint',
    'Reject path uses workflow command/API, not legacy entity endpoint',
    'Target entity status is updated by workflow target status updater or approved integration path',
    'Workflow history records submit/approve/reject actions append-only',
    'Notification/event evidence exists when workflow notification is expected',
    'Denied approver cannot approve or see another approver workflow item',
    'ESS read model shows workflow status/current step/current approver when required'
)

Write-Host 'Workflow verification checklist' -ForegroundColor Green
Write-Host "Entity type: $EntityType"
$required | ForEach-Object { Write-Host "[ ] $_" }

if ([string]::IsNullOrWhiteSpace($ApiBaseUrl)) {
    Write-Host "`nStatus: REVIEWED" -ForegroundColor Yellow
    Write-Host 'Reason: no -ApiBaseUrl supplied. Checklist generated only; no runtime workflow API verification was executed.'
    exit 0
}

$headers = @{}
if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
    $headers['Authorization'] = "Bearer $BearerToken"
}

$uri = ($ApiBaseUrl.TrimEnd('/') + '/' + $DefinitionEndpoint.TrimStart('/'))
Write-Host "`nChecking workflow definition endpoint: $uri" -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Method Get -Uri $uri -Headers $headers
    $json = $response | ConvertTo-Json -Depth 20

    if ($json -match [regex]::Escape($EntityType)) {
        Write-Host 'Definition endpoint responded and contains requested entity type.' -ForegroundColor Green
        Write-Host 'Status: PARTIAL'
        Write-Host 'Reason: definition endpoint check passed, but submit/start, approval, target status, history, notification, and denied-path checks still require scenario execution.'
        exit 0
    }

    Write-Host 'Status: FAILED' -ForegroundColor Red
    Write-Host "Reason: definition endpoint responded but did not contain entity type '$EntityType'."
    exit 1
}
catch {
    Write-Host 'Status: FAILED' -ForegroundColor Red
    Write-Host "Reason: workflow definition endpoint check failed: $($_.Exception.Message)"
    exit 1
}
