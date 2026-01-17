#!/usr/bin/env pwsh

<#
.SYNOPSIS
Generates RSA security key pairs for the Issuer and/or Client projects.

.DESCRIPTION
Creates RSA private/public PEM key pairs using OpenSSL.
Keys are written to the script directory, then copied into the appropriate
project runtime/secrets folders.

.PARAMETER Mode
ISSUER : Generates keys for WebApi.Issuer and publishes the public key to WebApi.Service.
CLIENT : Generates keys for WebApi.Client and publishes the public key to WebApi.Issuer.
ALL    : Generates both sets of keys (default).

.EXAMPLE
./generate-security-keys.ps1 -Mode ISSUER

.EXAMPLE
./generate-security-keys.ps1 -Mode CLIENT

.NOTES
Requires OpenSSL in PATH.
#>

using namespace System.IO

param(
    [ValidateSet("ISSUER", "CLIENT", "ALL")]
    [string]$Mode = "ALL"
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    function New-SecurityKeys {
        param(
            [Parameter(Mandatory)][string]$Prefix
        )

        $privatePem = Join-Path $PSScriptRoot "$Prefix-private.pem"
        $publicPem = Join-Path $PSScriptRoot "$Prefix-public.pem"

        & openssl genrsa -out $privatePem 2048
        & openssl rsa -in $privatePem -pubout -out $publicPem

        if ($Prefix -eq "issuer") {
            New-Item -ItemType Directory -Path $issuerSecretPath -Force | Out-Null
            Copy-Item $privatePem -Destination (Join-Path $issuerSecretPath "private-signing-key.pem") -Force
            Copy-Item $publicPem  -Destination (Join-Path $issuerSecretPath "public-signing-key.pem")  -Force
        }
        else {
            New-Item -ItemType Directory -Path $clientSecretPath -Force | Out-Null
            Copy-Item $privatePem -Destination (Join-Path $clientSecretPath "private-signing-key.pem") -Force

            New-Item -ItemType Directory -Path $jsonDataDir -Force | Out-Null
            Copy-Item $publicPem -Destination $jsonDataDir -Force

            New-Item -ItemType Directory -Path $postgresDataDir -Force | Out-Null
            Copy-Item $publicPem -Destination $postgresDataDir -Force
        }
    }

    # Project paths
    $src = Join-Path $PSScriptRoot "../../src"
    $projectSecretPath = "./runtime/secrets"

    $jsonDataDir = Join-Path $PSScriptRoot "../data/json"
    $postgresDataDir = Join-Path $PSScriptRoot "../data/postgres"

    $issuerSecretPath = Join-Path $src "WebApi.Issuer/$projectSecretPath"
    $clientSecretPath = Join-Path $src "WebApi.Client/$projectSecretPath"

    $generated = @()
}

process {
    if ($Mode -ne "CLIENT") {
        New-SecurityKeys -Prefix "issuer"
        $generated += "issuer"
    }

    if ($Mode -ne "ISSUER") {
        New-SecurityKeys -Prefix "client"
        $generated += "client"
    }
}

end {
    Write-Host "Security keys generated for $($generated -join ' and ')."
}