#!/bin/bash

set -e

dir="../src/GwentTracker/locale"

rm -f "$dir"/*.mo

msgfmt -o "$dir/cards.mo" "$dir/cards.pot"
msgfmt -o "$dir/gwent-tracker.mo" "$dir/gwent-tracker.pot"

find "$dir" -name "*.po" | \
while IFS= read -r file; do
    msgfmt -o "${file%.po}.mo" "${file}"
done
