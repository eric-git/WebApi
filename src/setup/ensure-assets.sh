#!/bin/sh
set -eu

if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    cat <<EOF
Usage: $(basename "$0") [--help]

Ensures all required cryptographic assets for the Web API suite are present.

This script validates and generates:
  - Issuer RSA keypair
  - Client RSA keypairs (from data/issuer-db.json)
  - Issuer hosting certificate
  - API hosting certificate

Missing assets are generated using:
  - generate-security-keys.sh
  - generate-hosting-cert.sh

All operations are idempotent and non-destructive.

Options:
  --help, -h   Show this help message and exit.

EOF
    exit 0
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SIGNING_DIR="$SCRIPT_DIR/signing"
HTTPS_DIR="$SCRIPT_DIR/https"
JSON_PATH="$SCRIPT_DIR/data/issuer-db.json"

mkdir -p "$SIGNING_DIR"
mkdir -p "$HTTPS_DIR"

test_keypair_exists() {
    prefix="$1"
    [ -f "$SIGNING_DIR/${prefix}-private.pem" ] && \
    [ -f "$SIGNING_DIR/${prefix}-public.pem" ]
}

test_certpair_exists() {
    prefix="$1"
    [ -f "$HTTPS_DIR/${prefix}-cert.crt" ] && \
    [ -f "$HTTPS_DIR/${prefix}-key.pem" ]
}

if [ ! -f "$JSON_PATH" ]; then
    echo "ERROR: issuer-db.json not found at $JSON_PATH" >&2
    exit 1
fi
CLIENT_IDS="$(jq -r '.Clients[].Id' "$JSON_PATH")"

ISSUER_PREFIX="ISSUER"
if ! test_keypair_exists "$ISSUER_PREFIX"; then
    echo "Generating issuer keys..."
    sh "$SCRIPT_DIR/generate-security-keys.sh" --mode ISSUER
else
    echo "Issuer keys already exist."
fi

for client_id in $CLIENT_IDS; do
    if ! test_keypair_exists "$client_id"; then
        echo "Generating client keys for $client_id..."
        sh "$SCRIPT_DIR/generate-security-keys.sh" --mode CLIENT --client-id "$client_id"
    else
        echo "Client keys for $client_id already exist."
    fi
done

if ! test_certpair_exists "issuer"; then
    echo "Generating issuer hosting certificate..."
    sh "$SCRIPT_DIR/generate-hosting-cert.sh" --mode ISSUER
else
    echo "Issuer hosting certificate already exists."
fi

if ! test_certpair_exists "api"; then
    echo "Generating API hosting certificate..."
    sh "$SCRIPT_DIR/generate-hosting-cert.sh" --mode API
else
    echo "API hosting certificate already exists."
fi

echo "Assets setup completed."