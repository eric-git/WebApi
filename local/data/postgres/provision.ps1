#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string] $PgHost = "localhost",
    [int]    $Port = 5432,

    [ValidateSet("ISSUER", "API", "ALL")]
    [string] $Mode = "ALL",

    [string] $BootstrapDbName = "postgres",
    [string] $BootstrapUser = "postgres",

    [string] $BootstrapPasswordFile
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    #
    # --- Helper: run SQL file with psql variables
    #
    function Invoke-PsqlFile {
        param(
            [Parameter(Mandatory)] [string] $File,
            [Parameter(Mandatory)] [hashtable] $Vars,
            [Parameter(Mandatory)] [string] $Database,
            [Parameter(Mandatory)] [string] $User,
            [Parameter(Mandatory)] [string] $Password
        )
        $File = (Resolve-Path $File).ProviderPath
        $env:PGPASSWORD = $Password
        try {
            $setArgs = @()
            foreach ($k in $Vars.Keys) {
                $setArgs += "--set"
                $setArgs += "$k=$($Vars[$k])"
            }

            psql `
                --no-psqlrc `
                --quiet `
                --host $PgHost `
                --port $Port `
                --username $User `
                --dbname $Database `
                --command "SET client_min_messages = warning;" `
                @setArgs `
                --file "$File"
        }
        finally {
            if (Test-Path Env:\PGPASSWORD) {
                Remove-Item Env:\PGPASSWORD
            }
        }
    }

    $Api = @{
        RoleName         = "svc_api"
        RolePasswordFile = Join-Path $PSScriptRoot "svc-api-password.txt"
        DbName           = "api_db"
        SchemaName       = "core"
    }
    $Issuer = @{
        RoleName         = "svc_issuer"
        RolePasswordFile = Join-Path $PSScriptRoot "svc-issuer-password.txt"
        DbName           = "issuer_db"
        SchemaName       = "core"
    }

    $dbManagerPasswordFile = Join-Path $PSScriptRoot "db-manager-password.txt"
    $dbManagerPassword = (Get-Content -Raw -Path $dbManagerPasswordFile).Trim()
    $clientPublicSigningKeyFile = Join-Path $PSScriptRoot "client-public.pem"
    $clientPublicSigningKey = (
            Get-Content $clientPublicSigningKeyFile |
            Where-Object {
                $_ -notmatch "^-----BEGIN PUBLIC KEY-----$" -and
                $_ -notmatch "^-----END PUBLIC KEY-----$"
            }
        ) -join ""
    $Apps = @()
    if ($Mode -eq "API" -or $Mode -eq "ALL") {
        $Apps += $Api 
    }
    if ($Mode -eq "ISSUER" -or $Mode -eq "ALL") { 
        $Apps += $Issuer 
    }
    if (-not $BootstrapPasswordFile) {
        $BootstrapPassword = Read-Host -Prompt "Enter password for bootstrap user '$BootstrapUser'" -MaskInput
    }
    else {
        $BootstrapPassword = (Get-Content -Raw -Path $BootstrapPasswordFile).Trim()
    }
}

process {
    #
    # --- Ensure db_manager exists and is SUPERUSER
    #
    Invoke-PsqlFile `
        -File (Join-Path $PSScriptRoot "ensure-db-manager.sql") `
        -Vars @{ role_password = $dbManagerPassword } `
        -Database $BootstrapDbName `
        -User $BootstrapUser `
        -Password $BootstrapPassword

    #
    # --- Switch context to db_manager
    #
    $CurrentUser = "db_manager"
    $CurrentPassword = $dbManagerPassword

    foreach ($App in $Apps) {
        #
        # --- Create roles
        #
        Invoke-PsqlFile `
            -File (Join-Path $PSScriptRoot "clean-service-account.sql") `
            -Vars @{ role_name = $App.RoleName } `
            -Database $App.DbName `
            -User $CurrentUser `
            -Password $CurrentPassword
        $rolePassword = (Get-Content -Raw -Path $App.RolePasswordFile).Trim()
        Invoke-PsqlFile `
            -File (Join-Path $PSScriptRoot "create-service-account.sql") `
            -Vars @{ role_name = $App.RoleName; role_password = $rolePassword } `
            -Database "postgres" `
            -User $CurrentUser `
            -Password $CurrentPassword

        #
        # --- Create DBs
        #
        Invoke-PsqlFile `
            -File (Join-Path $PSScriptRoot "create-db.sql") `
            -Vars @{ db_name = $App.DbName } `
            -Database "postgres" `
            -User $CurrentUser `
            -Password $CurrentPassword

        #
        # --- Configure schemas
        #
        $suffix = $App.RoleName.Split('_')[1]
        Invoke-PsqlFile `
            -File (Join-Path $PSScriptRoot "create-schema.sql") `
            -Vars @{ schema_name = $App.SchemaName; role_name = $App.RoleName } `
            -Database $App.DbName `
            -User $CurrentUser `
            -Password $CurrentPassword
        Invoke-PsqlFile `
            -File (Join-Path $PSScriptRoot "create-$suffix-schemas.sql") `
            -Vars @{} `
            -Database $App.DbName `
            -User $CurrentUser `
            -Password $CurrentPassword

        #
        # --- Seed data
        #
        Invoke-PsqlFile `
            -File (Join-Path $PSScriptRoot "seed-$suffix-data.sql") `
            -Vars @{ client_public_signing_key = $clientPublicSigningKey } `
            -Database $App.DbName `
            -User $CurrentUser `
            -Password $CurrentPassword
    }
}

end {
    Write-Host "Provisioning complete."
}
