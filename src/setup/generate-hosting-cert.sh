#!/bin/sh
set -eu

show_help() {
  cat <<EOF
SYNOPSIS
    Generates a self-signed hosting certificate for API or ISSUER projects.

DESCRIPTION
    This script automates the creation of a self-signed certificate and key pair
    using OpenSSL. It places the generated files into the https/ folder and copies
    them into the appropriate project's https/ directory. Supports both API and
    ISSUER modes with configurable validity period.

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
if [ "${1:-}" = "--help" ]; then
  show_help
  exit 0
fi

MODE="API"
VALID_DAYS=365

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
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

MODE_UPPER=$(echo "$MODE" | tr '[:lower:]' '[:upper:]')
MODE_LOWER=$(echo "$MODE" | tr '[:upper:]' '[:lower:]')
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
GENERATED_DIR="$SCRIPT_DIR/https"
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
    echo "Invalid mode specified. Use 'API' or 'ISSUER'."
    exit 1
    ;;
esac
CERT_PATH="$GENERATED_DIR/${MODE_LOWER}-cert.crt"
KEY_PATH="$GENERATED_DIR/${MODE_LOWER}-key.pem"

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
DEST_DIR="$SCRIPT_DIR/../$PROJECT_DIR/assets/https"
mkdir -p "$DEST_DIR"
cp -f "$CERT_PATH" "$DEST_DIR/cert.crt"
cp -f "$KEY_PATH" "$DEST_DIR/key.pem"
mkdir -p "$SCRIPT_DIR/../WebApi.Client/assets/https"
cp -f "$CERT_PATH" "$SCRIPT_DIR/../WebApi.Client/assets/https/${MODE_LOWER}-cert.crt"
if [ "$MODE_UPPER" = "ISSUER" ]; then
  mkdir -p "$SCRIPT_DIR/../WebApi.Service/assets/https"
  cp -f "$CERT_PATH" "$SCRIPT_DIR/../WebApi.Service/assets/https/${MODE_LOWER}-cert.crt"
fi
echo "Certificate for $MODE_LOWER generated."
