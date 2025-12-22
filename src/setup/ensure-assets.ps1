<#
.SYNOPSIS
Ensures all required cryptographic assets for the Web API suite are present.

.DESCRIPTION
This script validates and generates all security assets required by the Web API
ecosystem, including:

- Issuer RSA keypair
- Client RSA keypairs (based on issuer-db.json)
- Issuer hosting certificate
- API hosting certificate

The script checks for the existence of each asset in the local signing/ and https/
directories. If any required file is missing, the appropriate generator script is
invoked:

- generate-security-keys.ps1
- generate-hosting-cert.ps1

All operations are idempotent: existing assets are never overwritten unless the
generator scripts themselves choose to do so. Folder creation is deterministic
and non-destructive.

.PARAMETER None
This script takes no parameters. All configuration is derived from:

- The script's directory structure
- data/issuer-db.json
- The presence or absence of key/cert files

.EXAMPLE
PS> .\ensure-assets.ps1

Validates all issuer/client keys and hosting certificates, generating any missing
assets automatically.

.EXAMPLE
PS> pwsh ensure-assets.ps1

Runs the script from PowerShell Core, ensuring all cryptographic assets exist.

.NOTES
Requires: PowerShell 5+ or PowerShell Core  
Behavior:
- Uses strict mode for safety
- Uses explicit parameter names for clarity
- Uses $PSScriptRoot for deterministic path resolution
- Never deletes or overwrites existing assets unless generator scripts do so
#>

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"
    $signingDir = Join-Path -Path $PSScriptRoot -ChildPath "signing"
    $httpsDir = Join-Path -Path $PSScriptRoot -ChildPath "https"
    $jsonPath = Join-Path -Path $PSScriptRoot -ChildPath "data/issuer-db.json"
    New-Item -ItemType Directory -Force -Path $signingDir | Out-Null
    New-Item -ItemType Directory -Force -Path $httpsDir   | Out-Null

    function Test-KeyPairExists {
        param(
            [Parameter(Mandatory)]
            [string]$Prefix
        )
        $private = Join-Path -Path $signingDir -ChildPath "$Prefix-private.pem"
        $public = Join-Path -Path $signingDir -ChildPath "$Prefix-public.pem"
        return (Test-Path -Path $private -PathType Leaf) -and
        (Test-Path -Path $public  -PathType Leaf)
    }

    function Test-CertPairExists {
        param(
            [Parameter(Mandatory)]
            [string]$Prefix
        )
        $crt = Join-Path -Path $httpsDir -ChildPath "$Prefix-cert.crt"
        $pem = Join-Path -Path $httpsDir -ChildPath "$Prefix-key.pem"
        return (Test-Path -Path $crt -PathType Leaf) -and
        (Test-Path -Path $pem -PathType Leaf)
    }

    $settings = Get-Content -Path $jsonPath -Raw | ConvertFrom-Json
}

process {
    $issuerPrefix = "ISSUER"
    if (-not (Test-KeyPairExists -Prefix $issuerPrefix)) {
        Write-Host "Generating issuer keys..."
        & "$PSScriptRoot/generate-security-keys.ps1" -Mode "ISSUER"
    }
    else {
        Write-Host "Issuer keys already exist."
    }

    foreach ($client in $settings.Clients) {
        $clientId = $client.Id
        if (-not (Test-KeyPairExists -Prefix $clientId)) {
            Write-Host "Generating client keys for $clientId..."
            & "$PSScriptRoot/generate-security-keys.ps1" -Mode "CLIENT" -ClientId $clientId
        }
        else {
            Write-Host "Client keys for $clientId already exist."
        }
    }

    if (-not (Test-CertPairExists -Prefix "issuer")) {
        Write-Host "Generating missing issuer hosting certificate..."
        & "$PSScriptRoot/generate-hosting-cert.ps1" -Mode "ISSUER"
    }
    else {
        Write-Host "Issuer hosting certificate already exists."
    }

    if (-not (Test-CertPairExists -Prefix "api")) {
        Write-Host "Generating missing API hosting certificate..."
        & "$PSScriptRoot/generate-hosting-cert.ps1" -Mode "API"
    }
    else {
        Write-Host "API hosting certificate already exists."
    }
}
end {
    Write-Host "Assets setup completed."
}