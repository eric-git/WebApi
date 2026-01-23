#!/usr/bin/env pwsh

<#
.SYNOPSIS
  PowerShell equivalent of the Makefile for managing JSON and Postgres
  Docker Compose stacks. Provides commands for building, running, loading,
  resetting, inspecting, and destroying all related services.

.DESCRIPTION
  Available commands:

  JSON MODE
    json-build               Build all JSON services
    json-build-api           Build API (json)
    json-build-issuer        Build Issuer (json)
    json-build-client        Build Client (json)
    json-up                  Start JSON stack
    json-down                Stop JSON stack
    json-restart             Restart JSON stack
    json-ps                  Show JSON container status
    json-load                Run JSON loaders
    json-init                Full JSON init (build + load + up)
    json-init-up             Load then start JSON stack
    json-reset               Reset JSON mode (destroy volumes + re-init)
    json-logs                Tail JSON logs
    json-sh                  Shell into JSON service (requires -svc)

  POSTGRES MODE
    postgres-build           Build all Postgres services
    postgres-build-api       Build API (postgres)
    postgres-build-issuer    Build Issuer (postgres)
    postgres-build-client    Build Client (postgres)
    postgres-up              Start Postgres stack
    postgres-down            Stop Postgres stack
    postgres-restart         Restart Postgres stack
    postgres-ps              Show Postgres container status
    postgres-load            Run SQL loaders
    postgres-init            Full Postgres init (DBs + loaders + services)
    postgres-reset           Reset Postgres mode
    postgres-logs            Tail Postgres logs
    postgres-sh              Shell into Postgres service (requires -svc)

  UTILITIES
    secrets-check            Validate required secrets exist
    status                   Show system status (containers, volumes, networks)
    nuke                     Destroy all containers, volumes, images, networks
    help                     Show this help message

.PARAMETER Command
  The command to run (e.g., json-build, postgres-up, nuke, status).

.PARAMETER svc
  Optional service name for shell commands (json-sh, postgres-sh).

.EXAMPLE
  ./build.ps1 json-build

.EXAMPLE
  ./build.ps1 json-sh -svc api
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Command,

    [string]$svc
)

# =========================================
# Colors
# =========================================
$GREEN = "`e[32m"
$YELLOW = "`e[33m"
$BLUE = "`e[34m"
$RED = "`e[31m"
$RESET = "`e[0m"

# =========================================
# Helpers
# =========================================
function Section($msg) {
    Write-Host "$BLUE==> $msg$RESET"
    Write-Host ""
}

function Timed($scriptBlock) {
    $start = Get-Date
    & $scriptBlock
    $end = Get-Date
    $elapsed = [int]($end - $start).TotalSeconds
    Write-Host "$GREEN Completed in ${elapsed}s $RESET"
    Write-Host ""
}

function Require-Secret($path) {
    if (-not (Test-Path $path)) {
        Write-Host "$RED Missing secret: $path $RESET"
        exit 1
    }
}

# =========================================
# Compose wrapper
# =========================================
$COMPOSE = "docker compose"

# =========================================
# Base compose file
# =========================================
$BASE_FILE = "./docker/docker-compose.yml"

# =========================================
# Consolidated compose stacks
# =========================================
$COMPOSE_JSON = @(
    "-f $BASE_FILE",
    "-f docker/api/docker-compose.yml",
    "-f docker/api/docker-compose.json.yml",
    "-f docker/api/docker-compose.json.loader.yml",
    "-f docker/issuer/docker-compose.yml",
    "-f docker/issuer/docker-compose.json.yml",
    "-f docker/issuer/docker-compose.json.loader.yml",
    "-f docker/client/docker-compose.yml"
) -join " "

$COMPOSE_PG = @(
    "-f $BASE_FILE",
    "-f docker/api/docker-compose.yml",
    "-f docker/api/docker-compose.postgres.yml",
    "-f docker/api/docker-compose.postgres.loader.yml",
    "-f docker/issuer/docker-compose.yml",
    "-f docker/issuer/docker-compose.postgres.yml",
    "-f docker/issuer/docker-compose.postgres.loader.yml",
    "-f docker/client/docker-compose.yml"
) -join " "

# =========================================
# Command Implementations
# =========================================
switch ($Command) {

    # ---------------- JSON MODE ----------------
    "json-build" {
        Section "Building JSON stack..."
        Timed { iex "$COMPOSE $COMPOSE_JSON build" }
    }

    "json-build-api" {
        Section "Building API (json)..."
        Timed { iex "$COMPOSE $COMPOSE_JSON build api" }
    }

    "json-build-issuer" {
        Section "Building Issuer (json)..."
        Timed { iex "$COMPOSE $COMPOSE_JSON build issuer" }
    }

    "json-build-client" {
        Section "Building Client (json)..."
        Timed { iex "$COMPOSE $COMPOSE_JSON build client" }
    }

    "json-up" {
        Section "Starting JSON stack..."
        Timed { iex "$COMPOSE $COMPOSE_JSON up -d issuer api client" }
    }

    "json-down" {
        Section "Stopping JSON stack..."
        iex "$COMPOSE $COMPOSE_JSON down"
        Write-Host ""
    }

    "json-restart" {
        & $PSCommandPath json-down
        & $PSCommandPath json-up
    }

    "json-ps" {
        iex "$COMPOSE $COMPOSE_JSON ps"
        Write-Host ""
    }

    "json-load" {
        Section "Running JSON loaders..."
        Timed { iex "$COMPOSE $COMPOSE_JSON up --abort-on-container-exit issuer-json-loader api-json-loader" }
        iex "$COMPOSE $COMPOSE_JSON rm -f issuer-json-loader api-json-loader"
        Write-Host ""
    }

    "json-init" {
        & $PSCommandPath json-build
        & $PSCommandPath json-load
        & $PSCommandPath json-up
    }

    "json-init-up" {
        & $PSCommandPath json-load
        & $PSCommandPath json-up
    }

    "json-reset" {
        Section "Resetting JSON mode..."
        iex "$COMPOSE $COMPOSE_JSON down -v"
        & $PSCommandPath json-init
    }

    "json-logs" {
        Section "Aggregated logs (json)..."
        iex "$COMPOSE $COMPOSE_JSON logs -f"
    }

    "json-sh" {
        if (-not $svc) {
            Write-Host "$RED Usage: ./build.ps1 json-sh -svc api $RESET"
            exit 1
        }
        iex "$COMPOSE $COMPOSE_JSON exec $svc sh"
    }

    # ---------------- POSTGRES MODE ----------------
    "postgres-build" {
        Section "Building Postgres stack..."
        Timed { iex "$COMPOSE $COMPOSE_PG build" }
    }

    "postgres-build-api" {
        Section "Building API (postgres)..."
        Timed { iex "$COMPOSE $COMPOSE_PG build api" }
    }

    "postgres-build-issuer" {
        Section "Building Issuer (postgres)..."
        Timed { iex "$COMPOSE $COMPOSE_PG build issuer" }
    }

    "postgres-build-client" {
        Section "Building Client (postgres)..."
        Timed { iex "$COMPOSE $COMPOSE_PG build client" }
    }

    "postgres-up" {
        Section "Starting Postgres stack..."
        Timed { iex "$COMPOSE $COMPOSE_PG up -d postgres-issuer postgres-api issuer api client" }
    }

    "postgres-down" {
        Section "Stopping Postgres stack..."
        iex "$COMPOSE $COMPOSE_PG down"
        Write-Host ""
    }

    "postgres-restart" {
        & $PSCommandPath postgres-down
        & $PSCommandPath postgres-up
    }

    "postgres-ps" {
        iex "$COMPOSE $COMPOSE_PG ps"
        Write-Host ""
    }

    "postgres-load" {
        Section "Running SQL loaders..."
        Timed { iex "$COMPOSE $COMPOSE_PG up --abort-on-container-exit issuer-postgres-loader api-postgres-loader" }
        iex "$COMPOSE $COMPOSE_PG rm -f issuer-postgres-loader api-postgres-loader"
        Write-Host ""
    }

    "postgres-init" {
        Section "Postgres full initialization..."
        iex "$COMPOSE $COMPOSE_PG up -d postgres-issuer postgres-api"
        & $PSCommandPath postgres-load
        iex "$COMPOSE $COMPOSE_PG up -d issuer api client"
        Write-Host "$GREEN Postgres mode fully initialized $RESET"
    }

    "postgres-reset" {
        Section "Resetting Postgres mode..."
        iex "$COMPOSE $COMPOSE_PG down -v"
        & $PSCommandPath postgres-init
    }

    "postgres-logs" {
        Section "Aggregated logs (postgres)..."
        iex "$COMPOSE $COMPOSE_PG logs -f"
    }

    "postgres-sh" {
        if (-not $svc) {
            Write-Host "$RED Usage: ./build.ps1 postgres-sh -svc postgres-api $RESET"
            exit 1
        }
        iex "$COMPOSE $COMPOSE_PG exec $svc sh"
    }

    # ---------------- UTILITIES ----------------
    "secrets-check" {
        Section "Validating secrets..."
        Require-Secret "./secrets/db-manager-password.txt"
        Require-Secret "./secrets/svc-issuer-password.txt"
        Require-Secret "./secrets/svc-api-password.txt"
        Write-Host "$GREEN All required secrets are present $RESET"
        Write-Host ""
    }

    "status" {
        Section "System status..."
        docker ps
        Write-Host ""
        docker volume ls
        Write-Host ""
        docker network ls
        Write-Host ""
        Write-Host "$GREEN Status summary complete $RESET"
        Write-Host ""
    }

    "nuke" {
        Section "NUKING environment..."

        Write-Host "$YELLOW Removing containers... $RESET"
        docker ps -aq --filter "label=project=webapi-suite" | ForEach-Object { docker rm -f $_ }
        docker ps -a --format "{{.Names}}" | Select-String "^webapi-suite" | ForEach-Object { docker rm -f $_ }
        docker ps -aq | ForEach-Object {
            if (docker inspect $_ | Select-String '"webapi-suite-"' -Quiet) {
                docker rm -f $_
            }
        }

        Write-Host "$YELLOW Removing volumes... $RESET"
        docker volume ls -q | Select-String "^webapi-suite_" | ForEach-Object { docker volume rm -f $_ }

        Write-Host "$YELLOW Removing images... $RESET"
        docker images "webapi-suite/*" -q | ForEach-Object { docker rmi -f $_ }

        Write-Host "$YELLOW Pruning dangling images, volumes, build cache... $RESET"
        docker image prune -f
        docker volume prune -f
        docker builder prune -f

        Write-Host "$YELLOW Pruning unused networks... $RESET"
        docker network prune -f

        Write-Host "$GREEN Environment fully destroyed $RESET"
        Write-Host ""
    }

    "help" {
        Get-Help $PSCommandPath -Full
    }

    default {
        Write-Host "$RED Unknown command: $Command $RESET"
        Write-Host "Run: ./build.ps1 help"
        exit 1
    }
}
