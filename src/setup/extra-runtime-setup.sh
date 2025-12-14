#!/bin/sh
set -eu

if [ "$#" -eq 0 ]; then
    exit 1
fi
UPDATED=0
for CERT_SRC in "$@"; do
    CERT_BASENAME="$(basename "$CERT_SRC")"
    CERT_DST="/usr/local/share/ca-certificates/$CERT_BASENAME"
    if [ ! -f "$CERT_SRC" ]; then
        exit 1
    fi
    if [ -f "$CERT_DST" ] && cmp -s "$CERT_SRC" "$CERT_DST"; then
        continue
    fi
    install -m 0644 "$CERT_SRC" "$CERT_DST"
    UPDATED=1
done
if [ "$UPDATED" -eq 1 ]; then
    update-ca-certificates
fi
