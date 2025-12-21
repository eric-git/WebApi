<#
.SYNOPSIS
Resets data files for either ISSUER or API mode by copying the appropriate JSON
from the local data directory into the target WebApi project's data folder.

.DESCRIPTION
This script accepts a single parameter, Mode, which must be either "ISSUER" or "API".
Based on the selected mode, it locates the corresponding JSON file in the script's
data directory and copies it into the correct WebApi project folder, overwriting
any existing db.json file. It ensures strict error handling and provides a clear
confirmation message once the reset is complete.

.PARAMETER Mode
Specifies which project data to reset. Valid values are:
- ISSUER : Copies issuer-db.json into WebApi.Issuer/assets/data/db.json
- API    : Copies api-db.json into WebApi.Service/assets/data/db.json

.EXAMPLE
.\reset-db.ps1 -Mode ISSUER
Copies issuer-db.json into the WebApi.Issuer project's data folder.

.EXAMPLE
.\reset-db.ps1 -Mode API
Copies api-db.json into the WebApi.Service project's data folder.
#>

param(
    [Parameter()]
    [ValidateSet("ISSUER", "API")]
    [string]$Mode
)
begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"
    $sourceFile = "$PSScriptRoot/data/$($Mode.ToLower())-db.json"
}
process {
    switch ($Mode) {
        "ISSUER" {
            Copy-Item -Path $sourceFile -Destination "$PSScriptRoot/../WebApi.Issuer/assets/data/db.json" -Force
        }
        "API" {
            Copy-Item -Path $sourceFile -Destination "$PSScriptRoot/../WebApi.Service/assets/data/db.json" -Force
        }
        default {
            throw "Invalid mode specified. Use 'ISSUER' or 'CLIENT'."
        }
    }
}
end {
    Write-Host "Data for $($Mode.ToLower()) has been reset."
}