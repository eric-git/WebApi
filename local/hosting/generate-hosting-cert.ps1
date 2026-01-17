#!/usr/bin/env pwsh

<#
.SYNOPSIS
Generates self‑signed hosting certificates for API and/or ISSUER projects.

.DESCRIPTION
Creates a self‑signed X.509 certificate and private key using OpenSSL.
The generated certificate/key pair is written to the script directory and
then copied into each project's runtime/secrets folder. A combined CA bundle
is also produced for client and server trust.

.PARAMETER Mode
Specifies which certificates to generate:
- API     : Generate certs for WebApi.Service
- ISSUER  : Generate certs for WebApi.Issuer
- ALL     : Generate both (default)

.PARAMETER ValidDays
Number of days the certificate remains valid. Default: 365.

.EXAMPLE
.\generate-hosting-cert.ps1 -Mode API -ValidDays 90

.EXAMPLE
.\generate-hosting-cert.ps1 -Mode ISSUER

.NOTES
Requires OpenSSL to be available in PATH.
#>

param(
    [ValidateSet("API", "ISSUER", "ALL")]
    [string]$Mode = "ALL",

    [int]$ValidDays = 365
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    function Join-Paths {
        param(
            [Parameter(Mandatory)]
            [string[]]$Parts
        )

        $result = $Parts[0]
        foreach ($p in $Parts[1..($Parts.Count - 1)]) {
            $result = Join-Path $result $p
        }
        return $result
    }

    function New-HostingCert {
        param(
            [Parameter(Mandatory)][string]$AltName,
            [Parameter(Mandatory)][string]$SecretPath,
            [Parameter(Mandatory)][string]$CommonName
        )

        $certPath = Join-Path $PSScriptRoot "$AltName-cert.crt"
        $keyPath = Join-Path $PSScriptRoot "$AltName-key.pem"

        & openssl req `
            -x509 `
            -newkey rsa:2048 `
            -nodes `
            -out $certPath `
            -keyout $keyPath `
            -days $ValidDays `
            -subj "/C=AU/ST=ACT/L=Canberra/O=Project ERIC/OU=Web API Suite/CN=$CommonName" `
            -addext "subjectAltName=DNS:localhost,DNS:$AltName" `
            -addext "extendedKeyUsage=serverAuth"

        # Copy into project secret folder
        New-Item -ItemType Directory -Path $SecretPath -Force | Out-Null
        Copy-Item $certPath -Destination (Join-Path $SecretPath "hosting-cert.crt") -Force
        Copy-Item $keyPath  -Destination (Join-Path $SecretPath "hosting-key.pem") -Force
    }

    $src = Join-Path $PSScriptRoot "../../src"
    $projectSecretPath = "runtime/secrets"

    $issuerSecretPath = Join-Paths @($src, "WebApi.Issuer", $projectSecretPath)
    $apiSecretPath = Join-Paths @($src, "WebApi.Service", $projectSecretPath)
    $clientSecretPath = Join-Paths @($src, "WebApi.Client", $projectSecretPath)

    $caBundleFileName = Join-Path $PSScriptRoot "ca-bundle.crt"

    $generated = @()
}

process {
    if ($Mode -ne "API") {
        $alt = "issuer"
        New-HostingCert -AltName $alt `
            -SecretPath $issuerSecretPath `
            -CommonName "Token Issuer"

        $generated += $alt
    }

    if ($Mode -ne "ISSUER") {
        $alt = "api"
        New-HostingCert -AltName $alt `
            -SecretPath $apiSecretPath `
            -CommonName "Web API"

        $generated += $alt
    }

    $hostingCerts = Get-ChildItem -Path $PSScriptRoot -Filter "*-cert.crt"
    Get-Content $hostingCerts | Set-Content $caBundleFileName
    foreach ($dir in @($clientSecretPath, $apiSecretPath, $issuerSecretPath)) {
        $targetFileName = Join-Path $dir "ca-bundle.crt"
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Copy-Item $caBundleFileName $targetFileName -Force
    }
}

end {
    Write-Host "Generated certificates for $($generated -join ' and ')."
}
