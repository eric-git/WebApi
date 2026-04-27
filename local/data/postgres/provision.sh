#!/usr/bin/env sh
set -eu

# ------------------------------------------------------------
# Help
# ------------------------------------------------------------
print_help() {
    cat <<EOF
Usage: $0 [MODE] [options]

Provision PostgreSQL service databases.

Modes:
  API       Provision only the API database
  ISSUER    Provision only the ISSUER database
  ALL       Provision both databases (default)

Options:
  --host=HOST                     PostgreSQL host (default: localhost)
  --port=PORT                     PostgreSQL port (default: 5432)
  --bootstrap-user=USER           Superuser for provisioning (default: postgres)
  --bootstrap-password-file=FILE  Read bootstrap password from file
  --bootstrap-db=DB               Database to connect to as bootstrap (default: postgres)
  --help                          Show this help message
EOF
}

case "${1:-}" in
  --help|-h)
    print_help
    exit 0
    ;;
esac

# ------------------------------------------------------------
# Argument parsing
# ------------------------------------------------------------
mode="ALL"
host="localhost"
port="5432"
bootstrap_user="postgres"
bootstrap_password_file=""
bootstrap_db="postgres"

for arg in "$@"; do
    case "$arg" in
        API|ISSUER|ALL) mode="$arg" ;;
        --host=*) host="${arg#*=}" ;;
        --port=*) port="${arg#*=}" ;;
        --bootstrap-user=*) bootstrap_user="${arg#*=}" ;;
        --bootstrap-password-file=*) bootstrap_password_file="${arg#*=}" ;;
        --bootstrap-db=*) bootstrap_db="${arg#*=}" ;;
    esac
done

log() { printf '%s\n' "$*"; }

# ------------------------------------------------------------
# Paths and configuration
# ------------------------------------------------------------
script_dir="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"

api_role="svc_api"
api_role_password_file="$script_dir/svc-api-password.txt"
api_db="api_db"
api_schema="core"

issuer_role="svc_issuer"
issuer_role_password_file="$script_dir/svc-issuer-password.txt"
issuer_db="issuer_db"
issuer_schema="core"

db_manager_password_file="$script_dir/db-manager-password.txt"
db_manager_password="$(tr -d '\r\n' < "$db_manager_password_file")"

client_public_signing_key_file="$script_dir/client-public.pem"

# ------------------------------------------------------------
# Extract client public key (single line)
# ------------------------------------------------------------
extract_public_key() {
    awk '
        !/-----BEGIN PUBLIC KEY-----/ &&
        !/-----END PUBLIC KEY-----/ &&
        NF { printf "%s", $0 }
    ' "$1"
}

client_public_signing_key="$(extract_public_key "$client_public_signing_key_file")"

# ------------------------------------------------------------
# Bootstrap password handling
# ------------------------------------------------------------
if [ -n "$bootstrap_password_file" ]; then
    bootstrap_password="$(tr -d '\r\n' < "$bootstrap_password_file")"
else
    printf "Password for PostgreSQL superuser '%s': " "$bootstrap_user"
    stty -echo
    read bootstrap_password
    stty echo
    printf "\n"
fi

# ------------------------------------------------------------
# check if database exists
# ------------------------------------------------------------
db_exists() {
    local target_db="$1"
    PGPASSWORD="$bootstrap_password" \
    psql \
        --no-psqlrc \
        --quiet \
        --host "$host" \
        --port "$port" \
        --username "$bootstrap_user" \
        --dbname "$bootstrap_db" \
        --tuples-only \
        --command "SELECT 1 FROM pg_database WHERE datname = '$target_db';" |
        grep -q 1
}

# ------------------------------------------------------------
# psql execution with variable substitution
# ------------------------------------------------------------
invoke_psql_file() {
    file="$1"
    db="$2"
    user="$3"
    password="$4"
    shift 4

    # Check DB exists
    if ! db_exists "$db"; then
        return 0
    fi

    # Build --set args
    set_args=""
    for kv in "$@"; do
        set_args="$set_args --set $kv"
    done

    PGPASSWORD="$password" \
    psql \
        --no-psqlrc \
        --quiet \
        --host "$host" \
        --port "$port" \
        --username "$user" \
        --dbname "$db" \
        --command "SET client_min_messages = warning;" \
        $set_args \
        --file "$file"
}

# ------------------------------------------------------------
# App definitions
# ------------------------------------------------------------
apps=""

case "$mode" in
  API|ALL)    apps="$apps api" ;;
esac
case "$mode" in
  ISSUER|ALL) apps="$apps issuer" ;;
esac

# ------------------------------------------------------------
# Ensure db_manager exists
# ------------------------------------------------------------
invoke_psql_file \
    "$script_dir/ensure-db-manager.sql" \
    "$bootstrap_db" \
    "$bootstrap_user" \
    "$bootstrap_password" \
    "role_password=$db_manager_password"

current_user="db_manager"
current_password="$db_manager_password"

# ------------------------------------------------------------
# Process each app
# ------------------------------------------------------------
for app in $apps; do
    case "$app" in
        api)
            role_name="$api_role"
            role_password_file="$api_role_password_file"
            db_name="$api_db"
            schema_name="$api_schema"
            suffix="api"
            ;;
        issuer)
            role_name="$issuer_role"
            role_password_file="$issuer_role_password_file"
            db_name="$issuer_db"
            schema_name="$issuer_schema"
            suffix="issuer"
            ;;
    esac

    role_password="$(tr -d '\r\n' < "$role_password_file")"

    # 1. clean-service-account.sql
    invoke_psql_file \
        "$script_dir/clean-service-account.sql" \
        "$db_name" \
        "$current_user" \
        "$current_password" \
        "role_name=$role_name"

    # 2. create-service-account.sql
    invoke_psql_file \
        "$script_dir/create-service-account.sql" \
        "postgres" \
        "$current_user" \
        "$current_password" \
        "role_name=$role_name" \
        "role_password=$role_password"

    # 3. create-db.sql
    invoke_psql_file \
        "$script_dir/create-db.sql" \
        "postgres" \
        "$current_user" \
        "$current_password" \
        "db_name=$db_name"

    # 4. create-schema.sql
    invoke_psql_file \
        "$script_dir/create-schema.sql" \
        "$db_name" \
        "$current_user" \
        "$current_password" \
        "schema_name=$schema_name" \
        "role_name=$role_name"

    # 5. create-<suffix>-schemas.sql
    invoke_psql_file \
        "$script_dir/create-$suffix-schemas.sql" \
        "$db_name" \
        "$current_user" \
        "$current_password"

    # 6. seed-<suffix>-data.sql
    invoke_psql_file \
        "$script_dir/seed-$suffix-data.sql" \
        "$db_name" \
        "$current_user" \
        "$current_password" \
        "client_public_signing_key=$client_public_signing_key"
done

log "Provisioning complete."
