#!/bin/sh
set -eu

show_help() {
    cat <<EOF
SYNOPSIS
    Generates RSA security key pairs for either the Issuer or a Client project.

DESCRIPTION
    Automates the creation of RSA key pairs using OpenSSL.
    Keys are placed into signing/ and copied into the appropriate project folders.

OPTIONS
    --mode ISSUER|CLIENT
        ISSUER : Generates issuer keys and publishes public key to API.
        CLIENT : Generates client keys and publishes public key to Issuer.

    --client-id <GUID>
        Required when mode=CLIENT.

EXAMPLES
    $(basename "$0") --mode ISSUER
    $(basename "$0") --mode CLIENT --client-id 12345678-abcd-efgh-ijkl-9876543210
EOF
}

# Help flag
if [ "${1:-}" = "--help" ]; then
    show_help
    exit 0
fi

MODE="ISSUER"
CLIENT_ID=""

# -------------------------
# Parse arguments
# -------------------------
while [ $# -gt 0 ]; do
    case "$1" in
        --mode)
            MODE="$2"
            shift 2
            ;;
        --client-id)
            CLIENT_ID="$2"
            shift 2
            ;;
        *)
            printf '%s\n' "Unknown option: $1" >&2
            exit 1
            ;;
    esac
done

# Upper/lower conversion (POSIX)
MODE_UPPER=$(printf '%s' "$MODE" | tr '[:lower:]' '[:upper:]')
MODE_LOWER=$(printf '%s' "$MODE" | tr '[:upper:]' '[:lower:]')

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
SIGNING_DIR="$SCRIPT_DIR/signing"

ISSUER_DIR="$SCRIPT_DIR/../WebApi.Issuer/assets/signing"
CLIENT_DIR="$SCRIPT_DIR/../WebApi.Client/assets/signing"

# -------------------------
# Mode validation
# -------------------------
case "$MODE_UPPER" in
    ISSUER)
        PREFIX="issuer"
        ;;
    CLIENT)
        if [ -z "$CLIENT_ID" ]; then
            printf '%s\n' "CLIENT mode requires --client-id" >&2
            exit 1
        fi
        PREFIX="$CLIENT_ID"
        ;;
    *)
        printf '%s\n' "Invalid mode: $MODE" >&2
        exit 1
        ;;
esac

printf '%s\n' "Generating security keys for $MODE_LOWER..."

# -------------------------
# Generate keys
# -------------------------
mkdir -p "$SIGNING_DIR"

PRIVATE_PEM="$SIGNING_DIR/${PREFIX}-private.pem"
PUBLIC_PEM="$SIGNING_DIR/${PREFIX}-public.pem"

openssl genrsa -out "$PRIVATE_PEM" 2048
openssl rsa -in "$PRIVATE_PEM" -pubout -out "$PUBLIC_PEM"

# -------------------------
# Copy keys to projects
# -------------------------
mkdir -p "$ISSUER_DIR" "$CLIENT_DIR"

if [ "$MODE_UPPER" = "ISSUER" ]; then
    cp -f "$PRIVATE_PEM" "$ISSUER_DIR/private.pem"
    cp -f "$PUBLIC_PEM" "$ISSUER_DIR/public.pem"
else
    cp -f "$PRIVATE_PEM" "$CLIENT_DIR/private.pem"
    cp -f "$PUBLIC_PEM" "$ISSUER_DIR/${CLIENT_ID}-public.pem"
fi

printf '%s\n' "Security keys for $MODE_LOWER generated."
