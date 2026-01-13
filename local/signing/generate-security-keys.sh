#!/bin/sh
set -eu

show_help() {
    cat <<EOF
SYNOPSIS
    Generates RSA security key pairs for the Issuer and/or Client projects.

DESCRIPTION
    Produces RSA private/public PEM key pairs using OpenSSL.
    Keys are written to the script directory and then copied into the
    appropriate project runtime/secrets folders. Client public keys are
    also copied into the json and postgres data directories.

OPTIONS
    --mode ISSUER|CLIENT|ALL
        ISSUER : Generates keys for WebApi.Issuer and publishes the
                 public key to WebApi.Service.
        CLIENT : Generates keys for WebApi.Client and publishes the
                 public key to WebApi.Issuer.
        ALL    : Generates both sets of keys (default).

    --help
        Show this help message and exit.

EXAMPLES
    \$(basename "\$0") --mode ISSUER
    \$(basename "\$0") --mode CLIENT
    \$(basename "\$0") --mode ALL

REQUIREMENTS
    OpenSSL must be installed and available in PATH.
EOF
}

# Help flag
if [ "${1:-}" = "--help" ]; then
    show_help
    exit 0
fi

MODE="ALL"

# Parse arguments
while [ $# -gt 0 ]; do
    case "$1" in
        --mode)
            MODE="$2"
            shift 2
            ;;
        *)
            printf '%s\n' "Unknown option: $1" >&2
            exit 1
            ;;
    esac
done

MODE_UPPER=$(printf '%s' "$MODE" | tr '[:lower:]' '[:upper:]')

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
SRC_DIR="$SCRIPT_DIR/../../src"

# Data directories (match PowerShell)
JSON_DATA_DIR="$SCRIPT_DIR/../data/json"
POSTGRES_DATA_DIR="$SCRIPT_DIR/../data/postgres"

# Helper: join paths
join_paths() {
    out="$1"
    shift
    for p in "$@"; do out="$out/$p"; done
    printf '%s' "$out"
}

# Project secret paths
project_secret="runtime/secrets"

issuer_secret=$(join_paths "$SRC_DIR" "WebApi.Issuer" "$project_secret")
client_secret=$(join_paths "$SRC_DIR" "WebApi.Client" "$project_secret")
service_secret=$(join_paths "$SRC_DIR" "WebApi.Service" "$project_secret")

# Generate RSA key pair
generate_keys() {
    prefix="$1"
    private_dest="$2"
    public_dest="$3"

    private_pem="$SCRIPT_DIR/${prefix}-private.pem"
    public_pem="$SCRIPT_DIR/${prefix}-public.pem"

    openssl genrsa -out "$private_pem" 2048
    openssl rsa -in "$private_pem" -pubout -out "$public_pem"

    mkdir -p "$private_dest" "$public_dest"

    cp -f "$private_pem" "$private_dest/private-signing-key.pem"

    if [ "$prefix" = "issuer" ]; then
        cp -f "$public_pem" "$private_dest/public-signing-key.pem"
    else
        mkdir -p "$JSON_DATA_DIR" "$POSTGRES_DATA_DIR"
        cp -f "$public_pem" "$JSON_DATA_DIR"
        cp -f "$public_pem" "$POSTGRES_DATA_DIR"
    fi
}

generated=""

# ISSUER keys
if [ "$MODE_UPPER" != "CLIENT" ]; then
    generate_keys "issuer" "$issuer_secret" "$service_secret"
    generated="$generated issuer"
fi

# CLIENT keys
if [ "$MODE_UPPER" != "ISSUER" ]; then
    generate_keys "client" "$client_secret" "$issuer_secret"
    generated="$generated client"
fi

printf '%s\n' "Security keys generated for$(printf ' %s' $generated)."
