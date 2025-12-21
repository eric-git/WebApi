<#
.SYNOPSIS
Generates a self-signed hosting certificate for API or ISSUER projects.

.DESCRIPTION
This script automates the creation of a self-signed certificate and key pair
using OpenSSL. It places the generated files into https folder and copies
them into the appropriate project's https directory. Supports both API and
ISSUER modes with configurable validity period.

.PARAMETER Mode
Specifies which project to generate the certificate for.
Valid values are:
- API    : Generates certs for WebApi.Service
- ISSUER : Generates certs for WebApi.Issuer
Defaults to API.

.PARAMETER ValidDays
Specifies the number of days the certificate remains valid.
Default is 365 days.

.EXAMPLE
.\generate-hosting-cert.ps1 -Mode API -ValidDays 90
Generates a certificate for WebApi.Service valid for 90 days.

.EXAMPLE
.\generate-hosting-cert.ps1 -Mode ISSUER
Generates a certificate for WebApi.Issuer valid for 365 days.

.NOTES
Requires OpenSSL to be installed and available in PATH.
#>

param(
    [Parameter()]
    [ValidateSet("API", "ISSUER")]
    [string]$Mode = "API",
    [Parameter()]
    [int]$ValidDays = 365
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"
    $generatedFilePath = "$PSScriptRoot/https"
    $alternateName = $Mode.ToLower()
    switch ($Mode) {
        "API" {
            $projectDir = "WebApi.Service"
            $commonName = "Web API" 
        }
        "ISSUER" {
            $projectDir = "WebApi.Issuer"
            $commonName = "Token Issuer" 
        }
        default { throw "Invalid mode specified. Use 'API' or 'ISSUER'." }
    }
    $generatedCertPath = "$generatedFilePath/$alternateName-cert.crt"
    $generatedKeyPath = "$generatedFilePath/$alternateName-key.pem"
}
process {
    New-Item -ItemType Directory -Path $generatedFilePath -Force | Out-Null
    & openssl req `
        -x509 `
        -newkey rsa:2048 `
        -nodes `
        -out "$generatedCertPath" `
        -keyout "$generatedKeyPath" `
        -days $ValidDays `
        -subj "/C=AU/ST=ACT/L=Canberra/O=Project ERIC/OU=Web API Suite/CN=$commonName" `
        -addext "subjectAltName=DNS:localhost,DNS:$alternateName" `
        -addext "extendedKeyUsage=serverAuth"
    New-Item -ItemType Directory -Path "$PSScriptRoot/../$projectDir/assets/https" -Force | Out-Null
    Copy-Item -Path $generatedCertPath -Destination "$PSScriptRoot/../$projectDir/assets/https/cert.crt" -Force
    Copy-Item -Path $generatedKeyPath  -Destination "$PSScriptRoot/../$projectDir/assets/https/key.pem" -Force
    New-Item -ItemType Directory -Path "$PSScriptRoot/../WebApi.Client/assets/https" -Force | Out-Null
    Copy-Item -Path $generatedCertPath -Destination "$PSScriptRoot/../WebApi.Client/assets/https/$alternateName-cert.crt" -Force
    if ($Mode -eq "ISSUER") {
        New-Item -ItemType Directory -Path "$PSScriptRoot/../WebApi.Service/assets/https" -Force | Out-Null
        Copy-Item -Path $generatedCertPath -Destination "$PSScriptRoot/../WebApi.Service/assets/https/$alternateName-cert.crt" -Force
    }
}
end {
    Write-Host "Certificate for $($Mode.ToLower()) generated."
}
