#!/usr/bin/env pwsh

<#
.SYNOPSIS
Ensures all required cryptographic assets and service data for the Web API suite are present.

.DESCRIPTION
This script coordinates three independent, idempotent provisioning pipelines:

  1. Cryptographic assets:
       - Issuer RSA signing keypair
       - Client RSA signing keypairs (derived from issuer-db.json)
       - Issuer hosting certificate
       - API hosting certificate

  2. JSON-based service data:
       - Generates or validates JSON data files consumed by the services
       - Provides a lightweight, file-based alternative to PostgreSQL for local
         and development environments

  3. PostgreSQL service databases (optional):
       - If PostgreSQL is available, the database provisioning script is executed
       - If PostgreSQL is not available, the step is skipped with a diagnostic message

All invoked scripts are idempotent and non-destructive:
existing assets are preserved, and missing assets are created as needed.
#>

begin {
  Set-StrictMode -Version Latest
  $ErrorActionPreference = "Stop"

  $issuerSecretPath = Join-Path $PSScriptRoot "../src/WebApi.Issuer/runtime/secrets"
  $apiSecretPath = Join-Path $PSScriptRoot "../src/WebApi.Service/runtime/secrets"
}

process {
  & (Join-Path $PSScriptRoot "hosting/generate-hosting-cert.ps1")
  & (Join-Path $PSScriptRoot "signing/generate-security-keys.ps1")

  & (Join-Path $PSScriptRoot "data/json/provision.ps1")

  $postgresInstalled = $false
  try {
    $null = Get-Command psql -ErrorAction Stop
    $postgresInstalled = $true
  }
  catch {
    $postgresInstalled = $false
  }

  if ($postgresInstalled) {
    Write-Host "PostgreSQL detected — running database provisioning..."
    & (Join-Path $PSScriptRoot "data/postgres/provision.ps1")

    New-Item -ItemType Directory -Path $issuerSecretPath -Force | Out-Null
    Copy-Item (Join-Path $PSScriptRoot "data/postgres/svc-issuer-password.txt") `
    (Join-Path $issuerSecretPath "connection-default.password")

    New-Item -ItemType Directory -Path $apiSecretPath -Force | Out-Null
    Copy-Item (Join-Path $PSScriptRoot "data/postgres/svc-api-password.txt") `
    (Join-Path $apiSecretPath "connection-default.password")
  }
  else {
    Write-Warning "PostgreSQL not detected. Skipping PostgreSQL provisioning."
    Write-Host   "JSON provisioning has already completed and remains available as an alternative data source."
  }
}

end {
  Write-Host "All cryptographic assets and service data are verified and ready."
}
