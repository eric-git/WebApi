#!/bin/sh
set -eu

SRC_DIR="/app/https"
DST_DIR="/usr/local/share/ca-certificates"
UPDATED=0
for CERT_SRC in "$SRC_DIR"/*.crt; do
    CERT_BASENAME="$(basename "$CERT_SRC")"
    CERT_DST="$DST_DIR/$CERT_BASENAME"
    if [ -f "$CERT_DST" ] && cmp -s "$CERT_SRC" "$CERT_DST"; then
        continue
    fi
    install -m 0644 "$CERT_SRC" "$CERT_DST"
    UPDATED=1
done
if [ "$UPDATED" -eq 1 ]; then
    update-ca-certificates
fi
