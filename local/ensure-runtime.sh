#!/bin/sh
set -eu

show_help() {
    cat <<EOF
SYNOPSIS
    Orchestrates all provisioning steps required for the Web API suite,
    including cryptographic assets, JSON-based service data, and optional
    PostgreSQL databases.

DESCRIPTION
    This script coordinates three independent, idempotent provisioning
    pipelines:

      1. Cryptographic assets:
           - Hosting certificates for Issuer and API services
           - RSA signing keypairs for Issuer and Client
           - Public key propagation between dependent services

      2. JSON-based service data:
           - Generates or validates JSON data files used by the services
           - Provides a lightweight alternative to PostgreSQL for local
             and development environments

      3. PostgreSQL service databases (optional):
           - If PostgreSQL is installed, database provisioning is executed
           - If PostgreSQL is not installed, the step is skipped with a
             diagnostic message

    All invoked scripts are idempotent and non-destructive. Existing assets
    are preserved; missing assets are created as needed.

OPTIONS
    --help
        Show this help message and exit.

EXAMPLES
    $(basename "$0")
    $(basename "$0") --help

REQUIREMENTS
    OpenSSL must be installed and available in PATH.
    PostgreSQL is optional; JSON provisioning always runs.
EOF
}

# Help flag
if [ "\${1:-}" = "--help" ]; then
    show_help
    exit 0
fi

#!/bin/sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)

ISSUER_SECRET="$SCRIPT_DIR/../src/WebApi.Issuer/runtime/secrets"
API_SECRET="$SCRIPT_DIR/../src/WebApi.Service/runtime/secrets"

# -------------------------
# Helper: run a script safely
# -------------------------
run_script() {
    script="$1"
    if [ ! -f "$script" ]; then
        printf '%s\n' "Missing script: $script" >&2
        exit 1
    fi
    sh "$script"
}

# -------------------------
# Cryptographic provisioning
# -------------------------
run_script "$SCRIPT_DIR/hosting/generate-hosting-cert.sh"
run_script "$SCRIPT_DIR/signing/generate-security-keys.sh"

# -------------------------
# JSON data provisioning (always runs)
# -------------------------
run_script "$SCRIPT_DIR/data/json/provision.sh"

# -------------------------
# PostgreSQL detection
# -------------------------
postgres_installed=false
if command -v psql >/dev/null 2>&1; then
    postgres_installed=true
fi

# -------------------------
# PostgreSQL provisioning (optional)
# -------------------------
if [ "$postgres_installed" = true ]; then
    printf '%s\n' "PostgreSQL detected — running database provisioning..."

    run_script "$SCRIPT_DIR/data/postgres/provision.sh"

    mkdir -p "$ISSUER_SECRET"
    cp -f "$SCRIPT_DIR/data/postgres/svc-issuer-password.txt" \
          "$ISSUER_SECRET/connection-default.password"

    mkdir -p "$API_SECRET"
    cp -f "$SCRIPT_DIR/data/postgres/svc-api-password.txt" \
          "$API_SECRET/connection-default.password"

else
    printf '%s\n' "WARNING: PostgreSQL not detected. Skipping PostgreSQL provisioning." >&2
    printf '%s\n' "JSON provisioning has already completed and remains available as an alternative data source."
fi

printf '%s\n' "All cryptographic assets and data are verified and ready."
