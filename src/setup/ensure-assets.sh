#!/bin/sh
set -eu

# -------------------------
# Help
# -------------------------
case "${1:-}" in
    --help|-h)
        cat <<EOF
Usage: $(basename "$0") [--help]

Ensures all required cryptographic assets for the Web API suite are present.

Validates and generates:
  - Issuer RSA keypair
  - Client RSA keypairs (from data/issuer-db.json)
  - Issuer hosting certificate
  - API hosting certificate

Missing assets are generated using:
  - generate-security-keys.sh
  - generate-hosting-cert.sh

All operations are idempotent and non-destructive.
EOF
        exit 0
        ;;
esac

# -------------------------
# Paths
# -------------------------
SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
SIGNING_DIR="$SCRIPT_DIR/signing"
HTTPS_DIR="$SCRIPT_DIR/https"
JSON_PATH="$SCRIPT_DIR/data/issuer-db.json"

mkdir -p "$SIGNING_DIR" "$HTTPS_DIR"

# -------------------------
# Helpers
# -------------------------
test_keypair_exists() {
    prefix=$1
    [ -f "$SIGNING_DIR/${prefix}-private.pem" ] &&
    [ -f "$SIGNING_DIR/${prefix}-public.pem" ]
}

test_certpair_exists() {
    prefix=$1
    [ -f "$HTTPS_DIR/${prefix}-cert.crt" ] &&
    [ -f "$HTTPS_DIR/${prefix}-key.pem" ]
}

# -------------------------
# Validate JSON
# -------------------------
if [ ! -f "$JSON_PATH" ]; then
    printf '%s\n' "ERROR: issuer-db.json not found at $JSON_PATH" >&2
    exit 1
fi

CLIENT_IDS=$(jq -r '.Clients[].Id' "$JSON_PATH")

# -------------------------
# Issuer keypair
# -------------------------
if ! test_keypair_exists "ISSUER"; then
    printf '%s\n' "Generating issuer keys..."
    sh "$SCRIPT_DIR/generate-security-keys.sh" --mode ISSUER
else
    printf '%s\n' "Issuer keys already exist."
fi

# -------------------------
# Client keypairs
# -------------------------
for client_id in $CLIENT_IDS; do
    if ! test_keypair_exists "$client_id"; then
        printf '%s\n' "Generating client keys for $client_id..."
        sh "$SCRIPT_DIR/generate-security-keys.sh" --mode CLIENT --client-id "$client_id"
    else
        printf '%s\n' "Client keys for $client_id already exist."
    fi
done

# -------------------------
# Issuer hosting certificate
# -------------------------
if ! test_certpair_exists "issuer"; then
    printf '%s\n' "Generating issuer hosting certificate..."
    sh "$SCRIPT_DIR/generate-hosting-cert.sh" --mode ISSUER
else
    printf '%s\n' "Issuer hosting certificate already exists."
fi

# -------------------------
# API hosting certificate
# -------------------------
if ! test_certpair_exists "api"; then
    printf '%s\n' "Generating API hosting certificate..."
    sh "$SCRIPT_DIR/generate-hosting-cert.sh" --mode API
else
    printf '%s\n' "API hosting certificate already exists."
fi

printf '%s\n' "Assets setup completed."
