#!/usr/bin/env sh
set -eu

show_help() {
cat <<EOF
Scaffold EF Core DbContext and entity classes for API, ISSUER, or ALL.

USAGE:
  $(basename "$0") [--mode API|ISSUER|ALL]

OPTIONS:
  --mode    API, ISSUER, or ALL (default: ALL)

DESCRIPTION:
    Reads ConnectionStrings__Default from launchSettings.json and Password from a key-per-file secret,
    appends password to the connection string, and passes it to `dotnet ef dbcontext scaffold`.

EOF
}

# -----------------------------
# Defaults
# -----------------------------
MODE="ALL"

# -----------------------------
# Parse arguments
# -----------------------------
while [ $# -gt 0 ]; do
    case "$1" in
        --help|-h)
            show_help
            exit 0
            ;;
        --mode)
            MODE="$2"
            shift 2
            ;;
        *)
            echo "Unknown argument: $1" >&2
            exit 1
            ;;
    esac
done

echo "Starting EF Core scaffolding (RAW mode)..."

SRC="$(cd "$(dirname "$0")/../../../src" && pwd)"

# -----------------------------
# Project settings
# -----------------------------
API_CSPROJ="$SRC/WebApi.Service/WebApi.Service.csproj"
API_PROFILE="API - Postgres"

ISSUER_CSPROJ="$SRC/WebApi.Issuer/WebApi.Issuer.csproj"
ISSUER_PROFILE="Issuer - Postgres"

# -----------------------------
# Build raw connection string
# -----------------------------
get_raw_connection_string() {
    project_path="$1"
    profile="$2"

    launch_file="$(dirname "$project_path")/Properties/launchSettings.json"
    json="$(cat "$launch_file")"

    base_conn=$(printf "%s" "$json" | jq -r ".profiles[\"$profile\"].environmentVariables.ConnectionStrings__Default")
    secret_path=$(printf "%s" "$json" | jq -r ".profiles[\"$profile\"].environmentVariables.SECRET_PATH")

    # Resolve secret path relative to project if needed
    [ -d "$secret_path" ] || secret_path="$(dirname "$project_path")/$secret_path"

    password=$(cat "$secret_path/connection-default.password" | tr -d '\n')

    # Trim trailing semicolons from base_conn and append password
    base_conn=$(printf "%s" "$base_conn" | sed 's/;*$//')

    printf "%s;Password=%s" "$base_conn" "$password"
}

# -----------------------------
# Scaffold function
# -----------------------------
invoke_scaffold() {
    label="$1"
    csproj="$2"
    profile="$3"

    echo "=== Scaffolding $label ==="
    conn=$(get_raw_connection_string "$csproj" "$profile")

    echo "Project: $csproj"
    echo "Profile: $profile"
    echo "Schema:  core"

    dotnet ef dbcontext scaffold \
        "$conn" \
        Npgsql.EntityFrameworkCore.PostgreSQL \
        --project "$csproj" \
        --startup-project "$csproj" \
        --output-dir DataAccess/Entity \
        --context-dir DataAccess \
        --context AppDbContext \
        --schema core \
        --no-onconfiguring \
        --force
}

# -----------------------------
# Execute scaffolding
# -----------------------------
case "$MODE" in
    API|ALL)
        invoke_scaffold "API" "$API_CSPROJ" "$API_PROFILE"
        ;;
esac

case "$MODE" in
    ISSUER|ALL)
        invoke_scaffold "ISSUER" "$ISSUER_CSPROJ" "$ISSUER_PROFILE"
        ;;
esac

echo "Scaffolding completed."
