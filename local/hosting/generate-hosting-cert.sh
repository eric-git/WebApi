#!/bin/sh
set -eu

show_help() {
    cat <<EOF
SYNOPSIS
    Generates self‑signed hosting certificates for API and/or ISSUER projects.

DESCRIPTION
    Creates a self‑signed X.509 certificate and private key using OpenSSL.
    The generated certificate/key pair is written to the script directory and
    then copied into each project's runtime/secrets folder. A combined CA
    bundle is also produced for client and server trust.

OPTIONS
    --mode API|ISSUER|ALL
        API     : Generate certificates for WebApi.Service
        ISSUER  : Generate certificates for WebApi.Issuer
        ALL     : Generate both (default)

    --valid-days <N>
        Number of days the generated certificates remain valid.
        Default: 365 days.

    --help
        Show this help message and exit.

EXAMPLES
    \$(basename "\$0") --mode API --valid-days 90
    \$(basename "\$0") --mode ISSUER
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

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
SRC_DIR="$SCRIPT_DIR/../../src"

# -------------------------
# Helper: join paths
# -------------------------
join_paths() {
    out="$1"
    shift
    for p in "$@"; do
        out="$out/$p"
    done
    printf '%s' "$out"
}

# -------------------------
# Generate a hosting cert
# -------------------------
generate_cert() {
    alt_name="$1"
    secret_path="$2"
    common_name="$3"
    shift 3
    public_paths="$@"

    cert_path="$SCRIPT_DIR/${alt_name}-cert.crt"
    key_path="$SCRIPT_DIR/${alt_name}-key.pem"

    openssl req \
        -x509 \
        -newkey rsa:2048 \
        -nodes \
        -out "$cert_path" \
        -keyout "$key_path" \
        -days "$VALID_DAYS" \
        -subj "/C=AU/ST=ACT/L=Canberra/O=Project ERIC/OU=Web API Suite/CN=$common_name" \
        -addext "subjectAltName=DNS:localhost,DNS:$alt_name" \
        -addext "extendedKeyUsage=serverAuth"

    mkdir -p "$secret_path"
    cp -f "$cert_path" "$secret_path/hosting-cert.crt"
    cp -f "$key_path"  "$secret_path/hosting-key.pem"

    for pub in $public_paths; do
        mkdir -p "$pub"
        cp -f "$cert_path" "$pub/${alt_name}-hosting-cert.crt"
    done
}

# -------------------------
# Project secret paths
# -------------------------
project_secret="runtime/secrets"

issuer_secret=$(join_paths "$SRC_DIR" "WebApi.Issuer"  "$project_secret")
api_secret=$(join_paths    "$SRC_DIR" "WebApi.Service" "$project_secret")
client_secret=$(join_paths "$SRC_DIR" "WebApi.Client"  "$project_secret")

generated=""

# -------------------------
# Mode: ISSUER or ALL
# -------------------------
if [ "$MODE_UPPER" != "API" ]; then
    generate_cert "issuer" "$issuer_secret" "Token Issuer" "$client_secret" "$api_secret"
    generated="$generated issuer"
fi

# -------------------------
# Mode: API or ALL
# -------------------------
if [ "$MODE_UPPER" != "ISSUER" ]; then
    generate_cert "api" "$api_secret" "Web API" "$client_secret"
    generated="$generated api"
fi

# -------------------------
# Build CA bundle
# -------------------------
CA_BUNDLE="$SCRIPT_DIR/ca-bundle.crt"

: > "$CA_BUNDLE"
for f in "$SCRIPT_DIR"/*-cert.crt; do
    [ -f "$f" ] || continue
    cat "$f" >> "$CA_BUNDLE"
    printf '\n' >> "$CA_BUNDLE"
done

# Copy CA bundle into all project secret folders
for dir in "$client_secret" "$api_secret" "$issuer_secret"; do
    mkdir -p "$dir"
    cp -f "$CA_BUNDLE" "$dir/ca-bundle.crt"
done

printf '%s\n' "Generated certificates for$(printf ' %s' $generated)."
