<#
.SYNOPSIS
Ensures all required cryptographic assets for the Web API suite are present.

.DESCRIPTION
Validates and generates:
  - Issuer RSA keypair
  - Client RSA keypairs (from issuer-db.json)
  - Issuer hosting certificate
  - API hosting certificate

Missing assets are generated using:
  - generate-security-keys.ps1
  - generate-hosting-cert.ps1

All operations are idempotent and non-destructive.

.NOTES
Requires PowerShell 5+ or PowerShell Core.
#>

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    $root = $PSScriptRoot
    $signingDir = Join-Path $root "signing"
    $httpsDir = Join-Path $root "https"
    $jsonPath = Join-Path $root "data/issuer-db.json"

    New-Item -ItemType Directory -Force -Path $signingDir, $httpsDir | Out-Null

    function Test-KeyPair {
        param([string]$Prefix)
        Test-Path (Join-Path $signingDir "$Prefix-private.pem") -PathType Leaf -and
        Test-Path (Join-Path $signingDir "$Prefix-public.pem")  -PathType Leaf
    }

    function Test-CertPair {
        param([string]$Prefix)
        Test-Path (Join-Path $httpsDir "$Prefix-cert.crt") -PathType Leaf -and
        Test-Path (Join-Path $httpsDir "$Prefix-key.pem")  -PathType Leaf
    }

    if (-not (Test-Path $jsonPath -PathType Leaf)) {
        throw "issuer-db.json not found at $jsonPath"
    }
