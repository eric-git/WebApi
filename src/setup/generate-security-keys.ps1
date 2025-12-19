<#
.SYNOPSIS
Generates RSA security key pairs for either the Issuer or a Client project.

.DESCRIPTION
This script automates the creation of private/public PEM key pairs using OpenSSL.
It places the generated files into a signing folder and copies them into the
appropriate project directories. Supports both ISSUER and CLIENT modes.

.PARAMETER Mode
Specifies which project to generate keys for.
Valid values are:
- ISSUER : Generates keys for WebApi.Issuer and publishes the public key to WebApi.Service.
- CLIENT : Generates keys for WebApi.Client and publishes the public key to WebApi.Issuer.

.PARAMETER ClientId
Specifies the unique identifier for the client when Mode is CLIENT.
Defaults to "088f6a2e-8f00-4340-b32d-49de4acde03c".
Ignored when Mode is ISSUER.

.EXAMPLE
./generate-security-keys.ps1 -Mode ISSUER
Generates issuer keys and copies them into WebApi.Issuer and WebApi.Service.

.EXAMPLE
./generate-security-keys.ps1 -Mode CLIENT -ClientId "12345678-abcd-efgh-ijkl-9876543210"
Generates client keys and copies them into WebApi.Client and WebApi.Issuer.

.NOTES
Requires OpenSSL to be installed and available in PATH.
Outputs private.pem and public.pem files into the relevant project folders.
#>

param(
    [Parameter()]
    [ValidateSet("ISSUER", "CLIENT")]
    [string]$Mode = "ISSUER",
    [Parameter()]
    [string]$ClientId = "088f6a2e-8f00-4340-b32d-49de4acde03c"
)
begin {
    $ErrorActionPreference = "Stop"
    $scriptDir = Split-Path -Path $PSCommandPath -Parent
    $issuerProjectDir = "WebApi.Issuer"
    $clientProjectDir = "WebApi.Client"
    switch ($Mode) {
        "ISSUER" {
            $generatedFileNamePrefix = "issuer"
        }
        { $_ -eq "CLIENT" -and -not [string]::IsNullOrWhiteSpace($ClientId) } {
            $generatedFileNamePrefix = "$ClientId"
        }
        default {
            throw "Invalid mode specified. Use 'ISSUER' or 'CLIENT'."
        }
    }
    Write-Host "Generating security keys for $($Mode.ToLower())..."
}
process {
    $privatePemFilePath = "$scriptDir/signing/$generatedFileNamePrefix-private.pem"
    $publicPemFilePath = "$scriptDir/signing/$generatedFileNamePrefix-public.pem"
    & openssl genrsa -out $privatePemFilePath 2048
    & openssl rsa -in $privatePemFilePath -pubout -out $publicPemFilePath
    switch ($Mode) {
        "ISSUER" {
            Copy-Item -Path $privatePemFilePath -Destination "$scriptDir/../$issuerProjectDir/signing/private.pem" -Force
            Copy-Item -Path $publicPemFilePath -Destination "$scriptDir/../$issuerProjectDir/signing/public.pem" -Force
        }
        "CLIENT" {
            Copy-Item -Path $privatePemFilePath -Destination "$scriptDir/../$clientProjectDir/signing/private.pem" -Force
            Copy-Item -Path $publicPemFilePath -Destination "$scriptDir/../$issuerProjectDir/signing/$ClientId-public.pem" -Force
        }
    }
}
end {
    Write-Host "Security keys for $($Mode.ToLower()) generated."
}
