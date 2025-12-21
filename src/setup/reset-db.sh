#!/bin/sh
set -eu

show_help() {
  cat <<EOF
SYNOPSIS
    Reset data files for either ISSUER or API mode by copying the appropriate JSON
    from the local data directory into the target WebApi project's data folder.

DESCRIPTION
    This script accepts a single argument, MODE, which must be either "ISSUER" or "API".
    Based on the selected mode, it locates the corresponding JSON file in the script's
    data directory and copies it into the correct WebApi project folder, overwriting
    any existing db.json file. It ensures strict error handling and provides a clear
    confirmation message once the reset is complete.

USAGE
    $(basename "$0") ISSUER
        Copies issuer-db.json into WebApi.Issuer project's data folder.

    $(basename "$0") API
        Copies api-db.json into WebApi.Service project's data folder.

ARGUMENTS
    MODE    Must be one of:
            ISSUER - reset issuer data
            API    - reset API data
EOF
}
if [ "${1:-}" = "--help" ]; then
  show_help
  exit 0
fi
if [ $# -lt 1 ]; then
  echo "Error: MODE argument required (ISSUER or API)"
  exit 1
fi

MODE="$1"
MODE_LOWER=$(echo "$MODE" | tr '[:upper:]' '[:lower:]')
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SOURCE_FILE="$SCRIPT_DIR/data/${MODE_LOWER}-db.json"
case "$MODE" in
  ISSUER)
    cp -f "$SOURCE_FILE" "$SCRIPT_DIR/../WebApi.Issuer/assets/data/db.json"
    ;;
  API)
    cp -f "$SOURCE_FILE" "$SCRIPT_DIR/../WebApi.Service/assets/data/db.json"
    ;;
  *)
    echo "Invalid mode specified. Use 'ISSUER' or 'API'."
    exit 1
    ;;
esac
echo "Data for $MODE_LOWER has been reset."
