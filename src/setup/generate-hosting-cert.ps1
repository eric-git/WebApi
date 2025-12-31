<#
.SYNOPSIS
Generates a self-signed hosting certificate for API or ISSUER projects.

.DESCRIPTION
Creates a self-signed certificate and key pair using OpenSSL.
Outputs files into https/ and copies them into the appropriate project.

.PARAMETER Mode
API    : Generates certs for WebApi.Service
ISSUER : Generates certs for WebApi.Issuer
Default: API

.PARAMETER ValidDays
Certificate validity period (default: 365 days)

.EXAMPLE
.\generate-hosting-cert.ps1 -Mode API -ValidDays 90

.EXAMPLE
.\generate-hosting-cert.ps1 -Mode ISSUER

.NOTES
Requires OpenSSL in PATH.
#>

param(
    [ValidateSet("API", "ISSUER")]
    [string]$Mode = "API",

    [int]$ValidDays = 365
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    $root = $PSScriptRoot
    $httpsOut = Join-Path $root "https"
    $altName = $Mode.ToLower()

    switch ($Mode) {
        "API" {
            $projectDir = "WebApi.Service"
            $commonName = "Web API"
        }
        "ISSUER" {
            $projectDir = "WebApi.Issuer"
            $commonName = "Token Issuer"
        }
    }

    $certPath = Join-Path $httpsOut "$altName-cert.crt"
    $keyPath = Join-Path $httpsOut "$altName-key.pem"
}

process {
    # Ensure output folder exists
    New-Item -ItemType Directory -Path $httpsOut -Force | Out-Null

    # Generate certificate + key
    & openssl req `
        -x509 `
        -newkey rsa:2048 `
        -nodes `
        -out $certPath `
        -keyout $keyPath `
        -days $ValidDays `
        -subj "/C=AU/ST=ACT/L=Canberra/O=Project ERIC/OU=Web API Suite/CN=$commonName" `
        -addext "subjectAltName=DNS:localhost,DNS:$altName" `
        -addext "extendedKeyUsage=serverAuth"

    # Resolve project paths
    $projectHttps = Join-Path $root "..\$projectDir\assets\https"
    $clientHttps = Join-Path $root "..\WebApi.Client\assets\https"

    # Ensure directories exist
    New-Item -ItemType Directory -Path $projectHttps -Force | Out-Null
    New-Item -ItemType Directory -Path $clientHttps  -Force | Out-Null

    # Copy to primary project
    Copy-Item $certPath -Destination (Join-Path $projectHttps "cert.crt") -Force
    Copy-Item $keyPath  -Destination (Join-Path $projectHttps "key.pem")  -Force

    # Copy to client project
    Copy-Item $certPath -Destination (Join-Path $clientHttps "$altName-cert.crt") -Force

    # ISSUER also publishes to API
    if ($Mode -eq "ISSUER") {
        $apiHttps = Join-Path $root "..\WebApi.Service\assets\https"
        New-Item -ItemType Directory -Path $apiHttps -Force | Out-Null
        Copy-Item $certPath -Destination (Join-Path $apiHttps "$altName-cert.crt") -Force
    }
}

end {
    Write-Host "Certificate for $($Mode.ToLower()) generated."
}
