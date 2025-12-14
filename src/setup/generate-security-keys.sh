#!/bin/sh
set -eu

show_help() {
  cat <<'EOF'
SYNOPSIS
    Generates RSA security key pairs for either the Issuer or a Client project.

DESCRIPTION
    Automates the creation of RSA key pairs or self-signed certificates using OpenSSL.
    Keys and certs are placed into the signing/ or hosting/ folder and copied into
    the appropriate project directories.

OPTIONS
    --mode ISSUER|CLIENT
        Selects which project to generate keys for.
        ISSUER : Generates issuer keys and publishes public key to API.
        CLIENT : Generates client keys and publishes public key to Issuer.

    --client-id <GUID>
        Unique identifier for the client when mode is CLIENT.
        Ignored when mode is ISSUER.

    --valid-days <N>
        Number of days the certificate remains valid (for cert scripts).
        Default is 365.

EXAMPLES
    $(basename "$0") --mode ISSUER
        Generates issuer keys and copies them into WebApi.Issuer and WebApi.Service.

    $(basename "$0") --mode CLIENT --client-id 12345678-abcd-efgh-ijkl-9876543210
        Generates client keys and copies them into WebApi.Client and WebApi.Issuer.
EOF
}
if [[ "${1:-}" == "--help" ]]; then
  show_help
  exit 0
fi

MODE="ISSUER"
CLIENT_ID="088f6a2e-8f00-4340-b32d-49de4acde03c"
while [[ $# -gt 0 ]]; do
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
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done
MODE_UPPER=$(echo "$MODE" | tr '[:lower:]' '[:upper:]')
MODE_LOWER=$(echo "$MODE" | tr '[:upper:]' '[:lower:]')
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ISSUER_PROJECT_DIR="WebApi.Issuer"
API_PROJECT_DIR="WebApi.Service"
CLIENT_PROJECT_DIR="WebApi.Client"
case "$MODE_UPPER" in
  ISSUER)
    PREFIX="issuer"
    ;;
  CLIENT)
    if [[ -z "$CLIENT_ID" || "$CLIENT_ID" =~ ^[[:space:]]*$ ]]; then
      echo "Error: CLIENT mode requires a non-empty ClientId"
      exit 1
    fi
    PREFIX="$CLIENT_ID"
    ;;
  *)
    echo "Invalid mode specified. Use 'ISSUER' or 'CLIENT'."
    exit 1
    ;;
esac

echo "Generating security keys for $MODE_LOWER..."
PRIVATE_PEM="$SCRIPT_DIR/signing/${PREFIX}-private.pem"
PUBLIC_PEM="$SCRIPT_DIR/signing/${PREFIX}-public.pem"
mkdir -p "$SCRIPT_DIR/signing"
openssl genrsa -out "$PRIVATE_PEM" 2048
openssl rsa -in "$PRIVATE_PEM" -pubout -out "$PUBLIC_PEM"
case "$MODE_UPPER" in
  ISSUER)
    mkdir -p "$SCRIPT_DIR/../$ISSUER_PROJECT_DIR/signing" "$SCRIPT_DIR/../$API_PROJECT_DIR/signing"
    cp -f "$PRIVATE_PEM" "$SCRIPT_DIR/../$ISSUER_PROJECT_DIR/signing/private.pem"
    cp -f "$PUBLIC_PEM" "$SCRIPT_DIR/../$ISSUER_PROJECT_DIR/signing/public.pem"
    cp -f "$PUBLIC_PEM" "$SCRIPT_DIR/../$API_PROJECT_DIR/signing/issuer-public.pem"
    ;;
  CLIENT)
    mkdir -p "$SCRIPT_DIR/../$CLIENT_PROJECT_DIR/signing" "$SCRIPT_DIR/../$ISSUER_PROJECT_DIR/signing"
    cp -f "$PRIVATE_PEM" "$SCRIPT_DIR/../$CLIENT_PROJECT_DIR/signing/private.pem"
    cp -f "$PUBLIC_PEM" "$SCRIPT_DIR/../$ISSUER_PROJECT_DIR/signing/${CLIENT_ID}-public.pem"
    ;;
esac
echo "Security keys for $MODE_LOWER generated."
