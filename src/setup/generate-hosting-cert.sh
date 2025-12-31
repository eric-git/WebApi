#!/bin/sh
set -eu

show_help() {
    cat <<EOF
SYNOPSIS
    Generates a self-signed hosting certificate for API or ISSUER projects.

DESCRIPTION
    Creates a self-signed certificate and key pair using OpenSSL.
    Outputs files into https/ and copies them into the appropriate project.

OPTIONS
    --mode API|ISSUER
        API    : WebApi.Service
        ISSUER : WebApi.Issuer
        Default: API

    --valid-days <N>
        Certificate validity period (default: 365 days)

EXAMPLES
    $(basename "$0") --mode API --valid-days 90
    $(basename "$0") --mode ISSUER

REQUIREMENTS
    OpenSSL must be installed and available in PATH.
EOF
}

# Help flag
if [ "${1:-}" = "--help" ]; then
    show_help
    exit 0
fi

MODE="API"
VALID_DAYS=365

# -------------------------
# Parse arguments
# -------------------------
while [ $# -gt 0 ]; do
    case "$1" in
        --mode)
            MODE="$2"
            shift 2
            ;;
        --valid-days)
            VALID_DAYS="$2"
            shift 2
            ;;
        *)
            printf '%s\n' "Unknown option: $1" >&2
            exit 1
            ;;
    esac
done

# -------------------------
# Normalize mode
# -------------------------
MODE_UPPER=$(printf '%s' "$MODE" | tr '[:lower:]' '[:upper:]')
MODE_LOWER=$(printf '%s' "$MODE" | tr '[:upper:]' '[:lower:]')

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
GENERATED_DIR="$SCRIPT_DIR/https"

# -------------------------
# Mode selection
# -------------------------
case "$MODE_UPPER" in
    API)
        PROJECT_DIR="WebApi.Service"
        COMMON_NAME="Web API"
        ;;
    ISSUER)
        PROJECT_DIR="WebApi.Issuer"
        COMMON_NAME="Token Issuer"
        ;;
    *)
        printf '%s\n' "Invalid mode: $MODE" >&2
        exit 1
        ;;
esac

CERT_PATH="$GENERATED_DIR/${MODE_LOWER}-cert.crt"
KEY_PATH="$GENERATED_DIR/${MODE_LOWER}-key.pem"

# -------------------------
# Generate certificate
# -------------------------
mkdir -p "$GENERATED_DIR"

openssl req \
    -x509 \
    -newkey rsa:2048 \
    -nodes \
    -out "$CERT_PATH" \
    -keyout "$KEY_PATH" \
    -days "$VALID_DAYS" \
    -subj "/C=AU/ST=ACT/L=Canberra/O=Project ERIC/OU=Web API Suite/CN=$COMMON_NAME" \
    -addext "subjectAltName=DNS:localhost,DNS:$MODE_LOWER" \
    -addext "extendedKeyUsage=serverAuth"

# -------------------------
# Copy to primary project
# -------------------------
DEST_DIR="$SCRIPT_DIR/../$PROJECT_DIR/assets/https"
mkdir -p "$DEST_DIR"
cp -f "$CERT_PATH" "$DEST_DIR/cert.crt"
cp -f "$KEY_PATH" "$DEST_DIR/key.pem"

# -------------------------
# Copy to client project
# -------------------------
CLIENT_HTTPS="$SCRIPT_DIR/../WebApi.Client/assets/https"
mkdir -p "$CLIENT_HTTPS"
cp -f "$CERT_PATH" "$CLIENT_HTTPS/${MODE_LOWER}-cert.crt"

# -------------------------
# ISSUER also publishes to API
# -------------------------
if [ "$MODE_UPPER" = "ISSUER" ]; then
    API_HTTPS="$SCRIPT_DIR/../WebApi.Service/assets/https"
    mkdir -p "$API_HTTPS"
    cp -f "$CERT_PATH" "$API_HTTPS/${MODE_LOWER}-cert.crt"
fi

printf '%s\n' "Certificate for $MODE_LOWER generated."
