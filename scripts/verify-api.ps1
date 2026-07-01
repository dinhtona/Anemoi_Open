<#
.SYNOPSIS
  Runs a focused API smoke check with optional authorization header.

.DESCRIPTION
  This helper is intentionally generic. It validates one endpoint at a time and prints evidence suitable for an AI verification report.
  For security-sensitive changes, run it for both allowed and denied personas.

.EXAMPLE
  pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method GET -Path /api/hr/ess/profile -BearerToken $token -ExpectedStatus 200

.EXAMPLE
  pwsh ./scripts/verify-api.ps1 -ApiBaseUrl http://localhost:5000 -Method GET -Path /api/hr/employees -ExpectedStatus 401
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string]$ApiBaseUrl,
    [Parameter(Mandatory = $true)] [string]$Path,
    [ValidateSet('GET','POST','PUT','PATCH','DELETE')] [string]$Method = 'GET',
    [string]$BearerToken,
    [string]$BodyJson,
    [int]$ExpectedStatus = 200
)

$ErrorActionPreference = 'Stop'

$uri = $ApiBaseUrl.TrimEnd('/') + '/' + $Path.TrimStart('/')
$headers = @{}
if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
    $headers['Authorization'] = "Bearer $BearerToken"
}

Write-Host 'API verification helper' -ForegroundColor Green
Write-Host "Method: $Method"
Write-Host "URL: $uri"
Write-Host "Expected status: $ExpectedStatus"

try {
    $parameters = @{
        Method = $Method
        Uri = $uri
        Headers = $headers
        UseBasicParsing = $true
    }

    if (-not [string]::IsNullOrWhiteSpace($BodyJson)) {
        $parameters['Body'] = $BodyJson
        $parameters['ContentType'] = 'application/json'
    }

    $response = Invoke-WebRequest @parameters
    $actualStatus = [int]$response.StatusCode
}
catch {
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
        $actualStatus = [int]$_.Exception.Response.StatusCode
    }
    else {
        Write-Host 'Status: FAILED' -ForegroundColor Red
        Write-Host "Reason: request failed before receiving an HTTP response: $($_.Exception.Message)"
        exit 1
    }
}

Write-Host "Actual status: $actualStatus"

if ($actualStatus -eq $ExpectedStatus) {
    Write-Host 'Status: PASS' -ForegroundColor Green
    exit 0
}

Write-Host 'Status: FAILED' -ForegroundColor Red
Write-Host "Reason: expected HTTP $ExpectedStatus but got HTTP $actualStatus."
exit 1
