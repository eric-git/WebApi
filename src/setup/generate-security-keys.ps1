<#
.SYNOPSIS
Generates RSA security key pairs for either the Issuer or a Client project.

.DESCRIPTION
Automates creation of private/public PEM key pairs using OpenSSL.
Keys are stored in a signing folder and copied into the appropriate project directories.

.PARAMETER Mode
ISSUER : Generates keys for WebApi.Issuer and publishes public key to WebApi.Service.
CLIENT : Generates keys for WebApi.Client and publishes public key to WebApi.Issuer.

.PARAMETER ClientId
Required when Mode = CLIENT.

.EXAMPLE
./generate-security-keys.ps1 -Mode ISSUER

.EXAMPLE
./generate-security-keys.ps1 -Mode CLIENT -ClientId "12345678-abcd-efgh-ijkl-9876543210"

.NOTES
Requires OpenSSL in PATH.
#>

param(
    [ValidateSet("ISSUER", "CLIENT")]
    [string]$Mode = "ISSUER",

    [string]$ClientId
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    $issuerProjectDir = "WebApi.Issuer"
    $clientProjectDir = "WebApi.Client"

    switch ($Mode) {
        "ISSUER" {
            $Prefix = "issuer"
        }
        "CLIENT" {
            if ([string]::IsNullOrWhiteSpace($ClientId)) {
                throw "CLIENT mode requires a non-empty ClientId."
            }
            $Prefix = $ClientId
        }
    }

    Write-Host "Generating security keys for $($Mode.ToLower())..."
}

process {
    $signingRoot = Join-Path $PSScriptRoot "signing"
    $privatePem = Join-Path $signingRoot "$Prefix-private.pem"
    $publicPem = Join-Path $signingRoot "$Prefix-public.pem"

    # Ensure signing directory exists
    New-Item -ItemType Directory -Path $signingRoot -Force | Out-Null

    # Generate keys
    & openssl genrsa -out $privatePem 2048
    & openssl rsa -in $privatePem -pubout -out $publicPem

    # Resolve project signing directories
    $issuerSigning = Join-Path $PSScriptRoot "..\$issuerProjectDir\assets\signing"
    $clientSigning = Join-Path $PSScriptRoot "..\$clientProjectDir\assets\signing"

    # Ensure directories exist
    New-Item -ItemType Directory -Path $issuerSigning -Force | Out-Null
    New-Item -ItemType Directory -Path $clientSigning -Force | Out-Null

    if ($Mode -eq "ISSUER") {
        Copy-Item $privatePem -Destination (Join-Path $issuerSigning "private.pem") -Force
        Copy-Item $publicPem  -Destination (Join-Path $issuerSigning "public.pem")  -Force
    }
    else {
        Copy-Item $privatePem -Destination (Join-Path $clientSigning "private.pem") -Force
        Copy-Item $publicPem  -Destination (Join-Path $issuerSigning "$ClientId-public.pem") -Force
    }
}

end {
    Write-Host "Security keys for $($Mode.ToLower()) generated."
}
