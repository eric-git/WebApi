#!/bin/sh
set -eu

show_help() {
  cat <<EOF
SYNOPSIS
    Generates a self-signed hosting certificate for API or ISSUER projects.

DESCRIPTION
    Automates the creation of self-signed certificates using OpenSSL.
    Certificates and keys are placed into the hosting/ folder and copied
    into the appropriate project https/ directories.

OPTIONS
    --mode API|ISSUER
        Selects which project to generate the certificate for.
        API    : Generates certs for WebApi.Service
        ISSUER : Generates certs for WebApi.Issuer
        Default is API.

    --valid-days <N>
        Number of days the certificate remains valid.
        Default is 365.

EXAMPLES
    $(basename "$0") --mode API --valid-days 90
        Generates a certificate for WebApi.Service valid for 90 days.

    $(basename "$0") --mode ISSUER
        Generates a certificate for WebApi.Issuer valid for 365 days.

REQUIREMENTS
    OpenSSL must be installed and available in PATH.
EOF
}
if [[ "${1:-}" == "--help" ]]; then
  show_help
  exit 0
fi

MODE="API"
VALID_DAYS=365

while [[ $# -gt 0 ]]; do
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
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

MODE_UPPER=$(echo "$MODE" | tr '[:lower:]' '[:upper:]')
MODE_LOWER=$(echo "$MODE" | tr '[:upper:]' '[:lower:]')
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GENERATED_DIR="$SCRIPT_DIR/hosting"
case "$MODE_UPPER" in
  API)
    PROJECT_DIR="WebApi.Service"
    ;;
  ISSUER)
    PROJECT_DIR="WebApi.Issuer"
    ;;
  *)
    echo "Invalid mode specified. Use 'API' or 'ISSUER'."
    exit 1
    ;;
esac
CERT_PATH="$GENERATED_DIR/${MODE_LOWER}-cert.crt"
KEY_PATH="$GENERATED_DIR/${MODE_LOWER}-key.pem"

echo "Generating self-signed hosting certificate for $MODE_UPPER..."
mkdir -p "$GENERATED_DIR"
openssl req \
  -x509 \
  -newkey rsa:2048 \
  -nodes \
  -out "$CERT_PATH" \
  -keyout "$KEY_PATH" \
  -days "$VALID_DAYS" \
  -subj "/CN=$MODE_LOWER" \
  -addext "subjectAltName=DNS:localhost,DNS:$MODE_LOWER"
DEST_DIR="$SCRIPT_DIR/../$PROJECT_DIR/https"
mkdir -p "$DEST_DIR"
cp -f "$CERT_PATH" "$DEST_DIR/cert.crt"
cp -f "$KEY_PATH" "$DEST_DIR/key.pem"
cp -f "$CERT_PATH" "$SCRIPT_DIR/../WebApi.Client/https/${MODE_LOWER}-cert.crt"
echo "Certificate generated at $DEST_DIR"
