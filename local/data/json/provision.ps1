#!/usr/bin/env pwsh

<#
.SYNOPSIS
Resets development database files for ISSUER, API, or ALL modes.

.DESCRIPTION
Copies the corresponding *-db.json file from the script directory into each
WebApi project's runtime/data folder as a file named `db.data`. Existing files
are overwritten. Defaults to ALL.

.PARAMETER Mode
Specifies which project database(s) to reset:
- ISSUER : Copies issuer-db.json → WebApi.Issuer/runtime/data/db.data
- API    : Copies api-db.json    → WebApi.Service/runtime/data/db.data
- ALL    : Performs both operations (default)

.EXAMPLE
.\provision.ps1
Resets both issuer and API databases.

.EXAMPLE
.\provision.ps1 -Mode ISSUER
Resets only the issuer database.

.EXAMPLE
.\provision.ps1 -Mode API
Resets only the API database.
#>

param(
    [ValidateSet("ISSUER", "API", "ALL")]
    [string]$Mode = "ALL"
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    function Join-Paths {
        param([Parameter(Mandatory)][string[]]$Parts)

        $result = $Parts[0]
        foreach ($p in $Parts[1..($Parts.Count - 1)]) {
            $result = Join-Path $result $p
        }
        return $result
    }

    function Copy-Db {
        param(
            [Parameter(Mandatory)][string]$Prefix,
            [Parameter(Mandatory)][string]$DestinationDir
        )

        New-Item -ItemType Directory -Path $DestinationDir -Force | Out-Null

        $source = Join-Path $PSScriptRoot "$Prefix-db.json"
        $target = Join-Path $DestinationDir "db.data"

        Copy-Item -Path $source -Destination $target -Force
    }

    $src = Join-Path $PSScriptRoot "../../../src"
    $projectDataPath = "runtime/data"

    $issuerDataPath = Join-Paths @($src, "WebApi.Issuer", $projectDataPath)
    $apiDataPath = Join-Paths @($src, "WebApi.Service", $projectDataPath)

    $clientPublicSigningKeyFile = Join-Path $PSScriptRoot "client-public.pem"
    $clientPublicSigningKey = ((
            Get-Content $clientPublicSigningKeyFile |
            Where-Object {
                $_ -notmatch "^-----BEGIN PUBLIC KEY-----$" -and
                $_ -notmatch "^-----END PUBLIC KEY-----$"
            }
        ) -join "" | ConvertTo-Json -Compress).Trim('"')
    $templateKey = "{SAMPLE_CLIENT_PUBLIC_KEY}"
    
    $provisioned = @()
}

process {
    if ($Mode -ne "API") {
        $prefix = "issuer"

        $DataFile = $source = Join-Path $PSScriptRoot "$prefix-db.json"
        $templateFile = Join-Path (Split-Path $DataFile) ("{0}.template" -f (Split-Path $DataFile -LeafBase))
        (Get-Content $templateFile -Raw) `
            -replace [Regex]::Escape($templateKey), $clientPublicSigningKey |
        Set-Content $DataFile -Force

        Copy-Db -Prefix $prefix -DestinationDir $issuerDataPath
        $provisioned += $prefix
    }

    if ($Mode -ne "ISSUER") {
        $prefix = "api"
        Copy-Db -Prefix $prefix -DestinationDir $apiDataPath
        $provisioned += $prefix
    }
}

end {
    Write-Host "JSON data reset completed for $($provisioned -join ' and ')."
}
