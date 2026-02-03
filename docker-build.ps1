#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$Command,

    [string]$svc
)

# =========================================
# Colors
# =========================================
$Color = @{
    Green  = "`e[32m"
    Yellow = "`e[33m"
    Blue   = "`e[34m"
    Red    = "`e[31m"
    Reset  = "`e[0m"
}

function Write-Color($msg, $color = "Reset") {
    Write-Host "$($Color[$color])$msg$($Color.Reset)"
}

# =========================================
# Helpers
# =========================================
function Section($msg) {
    Write-Color "==> $msg" Blue
    Write-Host ""
}

function Timed([scriptblock]$Block) {
    $start = Get-Date
    try {
        & $Block
    }
    catch {
        Write-Color "Command failed: $_" Red
        exit 1
    }
    $elapsed = [int]((Get-Date) - $start).TotalSeconds
    Write-Color "Completed in ${elapsed}s" Green
    Write-Host ""
}

function Assert-Secret($path) {
    if (-not (Test-Path $path)) {
        Write-Color "Missing secret: $path" Red
        exit 1
    }
}

# =========================================
# Compose wrapper
# =========================================
function Compose {
    param(
        [array]$Files,
        [array]$Args
    )
    docker compose @Files @Args
}

# =========================================
# Compose stacks
# =========================================
$BASE_FILE = "./docker/docker-compose.yml"

$ComposeJson = @(
    "-f", $BASE_FILE,
    "-f", "docker/api/docker-compose.yml",
    "-f", "docker/api/docker-compose.json.yml",
    "-f", "docker/api/docker-compose.json.loader.yml",
    "-f", "docker/issuer/docker-compose.yml",
    "-f", "docker/issuer/docker-compose.json.yml",
    "-f", "docker/issuer/docker-compose.json.loader.yml",
    "-f", "docker/client/docker-compose.yml"
)

$ComposePg = @(
    "-f", $BASE_FILE,
    "-f", "docker/api/docker-compose.yml",
    "-f", "docker/api/docker-compose.postgres.yml",
    "-f", "docker/api/docker-compose.postgres.loader.yml",
    "-f", "docker/issuer/docker-compose.yml",
    "-f", "docker/issuer/docker-compose.postgres.yml",
    "-f", "docker/issuer/docker-compose.postgres.loader.yml",
    "-f", "docker/client/docker-compose.yml"
)

$ComposeJumpbox = @(
    "-f", $BASE_FILE,
    "-f", "docker/jumpbox/docker-compose.yml"
)

# =========================================
# JSON MODE
# =========================================
function json-build { Section "Building JSON stack..."; Timed { Compose $ComposeJson @("build") } }
function json-build-api { Section "Building API (json)..."; Timed { Compose $ComposeJson @("build", "api") } }
function json-build-issuer { Section "Building Issuer (json)..."; Timed { Compose $ComposeJson @("build", "issuer") } }
function json-build-client { Section "Building Client (json)..."; Timed { Compose $ComposeJson @("build", "client") } }

function json-up { Section "Starting JSON stack..."; Timed { Compose $ComposeJson @("up", "-d", "issuer", "api", "client") } }
function json-down { Section "Stopping JSON stack..."; Compose $ComposeJson @("down"); Write-Host "" }
function json-restart { json-down; json-up }

function json-ps { Compose $ComposeJson @("ps"); Write-Host "" }

function json-load {
    Section "Running JSON loaders..."
    Timed { Compose $ComposeJson @("up", "--abort-on-container-exit", "issuer-json-loader", "api-json-loader") }
    Compose $ComposeJson @("rm", "-f", "issuer-json-loader", "api-json-loader")
    Write-Host ""
}

function json-init { json-build; json-load; json-up }
function json-init-up { json-load; json-up }

function json-reset {
    Section "Resetting JSON mode..."
    Compose $ComposeJson @("down", "-v")
    json-init
}

function json-logs { Section "Aggregated logs (json)..."; Compose $ComposeJson @("logs", "-f") }

function json-sh {
    if (-not $svc) { Write-Color "Usage: ./docker-build.ps1 json-sh -svc api" Red; exit 1 }
    Compose $ComposeJson @("exec", $svc, "sh")
}

# =========================================
# POSTGRES MODE
# =========================================
function postgres-build { Section "Building Postgres stack..."; Timed { Compose $ComposePg @("build") } }
function postgres-build-api { Section "Building API (postgres)..."; Timed { Compose $ComposePg @("build", "api") } }
function postgres-build-issuer { Section "Building Issuer (postgres)..."; Timed { Compose $ComposePg @("build", "issuer") } }
function postgres-build-client { Section "Building Client (postgres)..."; Timed { Compose $ComposePg @("build", "client") } }

function postgres-up { Section "Starting Postgres stack..."; Timed { Compose $ComposePg @("up", "-d", "postgres-issuer", "postgres-api", "issuer", "api", "client") } }
function postgres-down { Section "Stopping Postgres stack..."; Compose $ComposePg @("down"); Write-Host "" }
function postgres-restart { postgres-down; postgres-up }

function postgres-ps { Compose $ComposePg @("ps"); Write-Host "" }

function postgres-load {
    Section "Running SQL loaders..."
    Timed { Compose $ComposePg @("up", "--abort-on-container-exit", "issuer-postgres-loader", "api-postgres-loader") }
    Compose $ComposePg @("rm", "-f", "issuer-postgres-loader", "api-postgres-loader")
    Write-Host ""
}

function postgres-init {
    Section "Postgres full initialization..."
    Compose $ComposePg @("up", "-d", "postgres-issuer", "postgres-api")
    postgres-load
    Compose $ComposePg @("up", "-d", "issuer", "api", "client")
    Write-Color "Postgres mode fully initialized" Green
}

function postgres-reset {
    Section "Resetting Postgres mode..."
    Compose $ComposePg @("down", "-v")
    postgres-init
}

function postgres-logs { Section "Aggregated logs (postgres)..."; Compose $ComposePg @("logs", "-f") }

function postgres-sh {
    if (-not $svc) { Write-Color "Usage: ./docker-build.ps1 postgres-sh -svc postgres-api" Red; exit 1 }
    Compose $ComposePg @("exec", $svc, "sh")
}

# =========================================
# JUMPBOX
# =========================================
function jumpbox-up { Section "Starting Jumpbox..."; Timed { Compose $ComposeJumpbox @("up", "-d", "jumpbox") } }
function jumpbox-down { Section "Stopping Jumpbox..."; Compose $ComposeJumpbox @("down"); Write-Host "" }
function jumpbox-restart { jumpbox-down; jumpbox-up }
function jumpbox-ps { Compose $ComposeJumpbox @("ps"); Write-Host "" }
function jumpbox-logs { Section "Jumpbox logs..."; Compose $ComposeJumpbox @("logs", "-f", "jumpbox") }
function jumpbox-sh { Section "Opening shell in Jumpbox..."; Compose $ComposeJumpbox @("exec", "jumpbox", "sh") }

# =========================================
# UTILITIES
# =========================================
function secrets-check {
    Section "Validating secrets..."
    Assert-Secret "./secrets/db-manager-password.txt"
    Assert-Secret "./secrets/svc-issuer-password.txt"
    Assert-Secret "./secrets/svc-api-password.txt"
    Write-Color "All required secrets are present" Green
    Write-Host ""
}

function status {
    Section "System status..."
    docker ps
    Write-Host ""
    docker volume ls
    Write-Host ""
    docker network ls
    Write-Host ""
    Write-Color "Status summary complete" Green
    Write-Host ""
}

function nuke {
    Section "NUKING environment..."

    Write-Color "Removing containers..." Yellow
    docker ps -aq --filter "label=project=webapi-suite" | ForEach-Object { docker rm -f $_ }
    docker ps -a --format "{{.Names}}" | Select-String "^webapi-suite" | ForEach-Object { docker rm -f $_ }
    docker ps -aq | ForEach-Object {
        if (docker inspect $_ | Select-String '"webapi-suite-"' -Quiet) {
            docker rm -f $_
        }
    }

    Write-Color "Removing volumes..." Yellow
    docker volume ls -q | Select-String "^webapi-suite_" | ForEach-Object { docker volume rm -f $_ }

    Write-Color "Removing images..." Yellow
    docker images "webapi-suite/*" -q | ForEach-Object { docker rmi -f $_ }

    Write-Color "Pruning dangling images, volumes, build cache..." Yellow
    docker image prune -f
    docker volume prune -f
    docker builder prune -f

    Write-Color "Pruning unused networks..." Yellow
    docker network prune -f

    Write-Color "Environment fully destroyed" Green
    Write-Host ""
}

function help {
    Write-Color "Available commands:" Yellow
    Write-Host ""
    Write-Color "JSON MODE" Blue
    "json-build", "json-build-api", "json-build-issuer", "json-build-client",
    "json-up", "json-down", "json-restart", "json-ps", "json-load",
    "json-init", "json-init-up", "json-reset", "json-logs", "json-sh" |
    ForEach-Object { "  $_" }
    Write-Host ""
    Write-Color "POSTGRES MODE" Blue
    "postgres-build", "postgres-build-api", "postgres-build-issuer", "postgres-build-client",
    "postgres-up", "postgres-down", "postgres-restart", "postgres-ps",
    "postgres-load", "postgres-init", "postgres-reset", "postgres-logs", "postgres-sh" |
    ForEach-Object { "  $_" }
    Write-Host ""
    Write-Color "JUMPBOX" Blue
    "jumpbox-up", "jumpbox-down", "jumpbox-restart", "jumpbox-ps", "jumpbox-logs", "jumpbox-sh" |
    ForEach-Object { "  $_" }
    Write-Host ""
    Write-Color "UTILITIES" Blue
    "secrets-check", "status", "nuke", "help" |
    ForEach-Object { "  $_" }
    Write-Host ""
}

# =========================================
# DISPATCHER
# =========================================
$Commands = @{
    # JSON
    "json-build" = "json-build"; "json-build-api" = "json-build-api"; "json-build-issuer" = "json-build-issuer"; "json-build-client" = "json-build-client";
    "json-up" = "json-up"; "json-down" = "json-down"; "json-restart" = "json-restart"; "json-ps" = "json-ps";
    "json-load" = "json-load"; "json-init" = "json-init"; "json-init-up" = "json-init-up"; "json-reset" = "json-reset";
    "json-logs" = "json-logs"; "json-sh" = "json-sh";

    # POSTGRES
    "postgres-build" = "postgres-build"; "postgres-build-api" = "postgres-build-api"; "postgres-build-issuer" = "postgres-build-issuer"; "postgres-build-client" = "postgres-build-client";
    "postgres-up" = "postgres-up"; "postgres-down" = "postgres-down"; "postgres-restart" = "postgres-restart"; "postgres-ps" = "postgres-ps";
    "postgres-load" = "postgres-load"; "postgres-init" = "postgres-init"; "postgres-reset" = "postgres-reset";
    "postgres-logs" = "postgres-logs"; "postgres-sh" = "postgres-sh";

    # JUMPBOX
    "jumpbox-up" = "jumpbox-up"; "jumpbox-down" = "jumpbox-down"; "jumpbox-restart" = "jumpbox-restart";
    "jumpbox-ps" = "jumpbox-ps"; "jumpbox-logs" = "jumpbox-logs"; "jumpbox-sh" = "jumpbox-sh";

    # UTILITIES
    "secrets-check" = "secrets-check"; "status" = "status"; "nuke" = "nuke"; "help" = "help"
}

if ($Commands.ContainsKey($Command)) {
    & $Commands[$Command]
}
else {
    Write-Color "Unknown command: $Command" Red
    Write-Color "Run: ./docker-build.ps1 help" Yellow
    exit 1
}
