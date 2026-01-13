#!/bin/sh
set -eu

show_help() {
    cat <<EOF
SYNOPSIS
    Resets development database files for ISSUER, API, or ALL modes.

DESCRIPTION
    Copies the corresponding *-db.json file from the script directory into each
    WebApi project's runtime/data folder as a file named 'db.data'. Existing files
    are overwritten. Defaults to ALL.

OPTIONS
    --mode ISSUER|API|ALL
        ISSUER : Copies issuer-db.json → WebApi.Issuer/runtime/data/db.data
        API    : Copies api-db.json    → WebApi.Service/runtime/data/db.data
        ALL    : Performs both operations (default)

EXAMPLES
    \$(basename "\$0")
    \$(basename "\$0") --mode ISSUER
    \$(basename "\$0") --mode API
EOF
}

# Help flag
if [ "${1:-}" = "--help" ]; then
    show_help
    exit 0
fi

MODE="ALL"

# -------------------------
# Parse arguments
# -------------------------
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
SRC_DIR="$SCRIPT_DIR/../../../src"

# -------------------------
# Helper: join paths
# -------------------------
join_paths() {
    out="$1"
    shift
    for p in "$@"; do out="$out/$p"; done
    printf '%s' "$out"
}

# -------------------------
# Copy a database file
# -------------------------
copy_db() {
    prefix="$1"
    dest_dir="$2"

    mkdir -p "$dest_dir"

    src_file="$SCRIPT_DIR/${prefix}-db.json"
    dest_file="$dest_dir/db.data"

    cp -f "$src_file" "$dest_file"
}

# -------------------------
# Project data paths
# -------------------------
issuer_data=$(join_paths "$SRC_DIR" "WebApi.Issuer"  "runtime/data")
api_data=$(join_paths    "$SRC_DIR" "WebApi.Service" "runtime/data")

provisioned=""

# -------------------------
# Load client public key (single line, no header/footer)
# -------------------------
client_pub_file="$SCRIPT_DIR/client-public.pem"
client_pub_key=$(
    awk '
        !/-----BEGIN PUBLIC KEY-----/ &&
        !/-----END PUBLIC KEY-----/ &&
        NF { printf "%s", $0 }
    ' "$client_pub_file"
)

# Escape for sed replacement
escaped_key=$(printf '%s' "$client_pub_key" | sed 's/[&/]/\\&/g')

template_key="{SAMPLE_CLIENT_PUBLIC_KEY}"

# -------------------------
# ISSUER (unless API-only)
# -------------------------
if [ "$MODE_UPPER" != "API" ]; then
    template="$SCRIPT_DIR/issuer-db.template"
    output="$SCRIPT_DIR/issuer-db.json"

    sed "s|$template_key|$escaped_key|g" "$template" > "$output"

    copy_db "issuer" "$issuer_data"
    provisioned="$provisioned issuer"
fi

# -------------------------
# API (unless ISSUER-only)
# -------------------------
if [ "$MODE_UPPER" != "ISSUER" ]; then
    copy_db "api" "$api_data"
    provisioned="$provisioned api"
fi

printf '%s\n' "JSON data reset completed for$(printf ' %s' $provisioned)."
