#!/bin/bash

set -e

scriptName=$(basename "$0")

if [[ ${#@} = 0 ]]; then
    locales=("ar" "cs" "de" "es" "es-mx" "fr" "hu" "it" "ja" "ko" "pl" "pt-br" "ru" "tr" "zh" "zh-cn")
else
    locales=($@)
fi

. to-po.sh

function prepareId() {
    echo "$1" | sed 's/\([*]\)/\\\1/g'
}

addMiscToMap
writePoFiles gwent-tracker
printMissing

