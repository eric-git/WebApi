#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Scaffolds EF Core DbContext and entities for API and/or ISSUER projects using RAW PostgreSQL connection strings.

.DESCRIPTION
    Reads ConnectionStrings__Default from launchSettings.json and Password from a key-per-file secret,
    appends password to the connection string, and passes it to `dotnet ef dbcontext scaffold`.

.PARAMETER Mode
    Which project(s) to scaffold: API, ISSUER, or ALL (default).
#>

[CmdletBinding()]
param(
    [ValidateSet("API", "ISSUER", "ALL")]
    [string]$Mode = "ALL"
)

Write-Host "Starting EF Core scaffolding (RAW connection mode)..."

$src = Join-Path $PSScriptRoot "../../../src"

$Projects = @{
    API    = @{
        CsProj  = Join-Path $src "WebApi.Service/WebApi.Service.csproj"
        Profile = "Api (Postgres Data)"
    }
    ISSUER = @{
        CsProj  = Join-Path $src "WebApi.Issuer/WebApi.Issuer.csproj"
        Profile = "Issuer (Postgres Data)"
    }
}

function Get-RawConnectionString {
    param(
        [string]$CsProjPath,
        [string]$ProfileName
    )

    $launchPath = Join-Path (Split-Path $CsProjPath) "Properties/launchSettings.json"
    $launch = Get-Content $launchPath -Raw | ConvertFrom-Json
    $env = $launch.profiles.$ProfileName.environmentVariables

    $baseConn = $env."ConnectionStrings__Default"
    $secretPath = $env."SECRET_PATH"

    if (!(Test-Path $secretPath)) {
        $secretPath = Join-Path (Split-Path $CsProjPath) $secretPath
    }

    $password = (Get-Content (Join-Path $secretPath "connection-default.password") -Raw).Trim()

    "$($baseConn.TrimEnd(';'));Password=$password"
}

function Invoke-Scaffold {
    param(
        [string]$Label,
        [hashtable]$Project
    )

    Write-Host "=== Scaffolding $Label ==="

    $conn = Get-RawConnectionString -CsProjPath $Project.CsProj -ProfileName $Project.Profile

    & dotnet ef dbcontext scaffold `
        "$conn" `
        Npgsql.EntityFrameworkCore.PostgreSQL `
        --project $Project.CsProj `
        --startup-project $Project.CsProj `
        --context AppDbContext `
        --schema core `
        --output-dir DataAccess/Entity `
        --context-dir DataAccess `
        --no-onconfiguring `
        --force
}

if ($Mode -in @("API", "ALL")) {
    Invoke-Scaffold -Label "API" -Project $Projects.API
}

if ($Mode -in @("ISSUER", "ALL")) {
    Invoke-Scaffold -Label "ISSUER" -Project $Projects.ISSUER
}

Write-Host "Scaffolding completed."
