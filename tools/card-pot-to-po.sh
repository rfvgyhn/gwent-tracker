#!/bin/bash

set -e

# Depends on:
#    jq - https://stedolan.github.io/jq/
#    xml-to-json - https://github.com/sinelaw/xml-to-json

scriptName=$(basename "$0")
data="/mnt/secondary/witcher3/uncooked-data/base/gameplay/items"
hashMap="card-id-hash-map.txt"

if [[ ${#@} = 0 ]]; then
    locales=("ar" "cs" "de" "es" "es-mx" "fr" "hu" "it" "ja" "ko" "pl" "pt-br" "ru" "tr" "zh" "zh-cn")
else
    locales=($@)
fi

. to-po.sh

function prepareId() {
    echo "$1"
}

function jqstr() {
   echo ".redxml.custom.gwint_${1}.card[]"
}

function getHashFromId() {
    grep "$1$" "$hashMap" | head -n1 | cut -d '|' -f 1
}

function getEnglishString() {
    find "$strings" -name "en*.csv" |\
    xargs -n1 grep "${1}" 2>/dev/null |\
    head -n1 |\
    cut -d '|' -f 1,4 |\
    cleanStr ${2}
}

function addCardsToMap() {
    echo "Adding cards to map..."
    while read line; do
        IFS=' ' read -ra parts <<< "$line"
        local index=${parts[0]}
        local titleKey=${parts[1]}
        local descKey=${parts[2]}
        
        local titleHash=""
        case ${titleKey} in
            Mysterious) titleKey=gwint_name_avallach;descKey=gwint_desc_avallach;;
            Crach_an_Craite) titleHash=f3a32552;;
        esac
        
        [[ "$titleHash" = "" ]] && titleHash=$(getHashFromId "$titleKey")
        local descHash=$(getHashFromId "$descKey")
        
        addToMap "$(getEnglishString ${titleHash} true)"
        addToMap "$(getEnglishString ${descHash} false)"
    done <<< $({ xml-to-json "${data}/def_gwint_battle_king_cards.xml" | jq -r "$(jqstr battle_king_card_definitions)"; \
                 xml-to-json "${data}/def_gwint_cards_final.xml" | jq "$(jqstr card_definitions_final)"; \
                 echo "{ \"index\": \"145\", \"title\": \"gwint_name_ballista\", \"description\": \"gwint_desc_ballista\" }"; \
                 echo "{ \"index\": \"146\", \"title\": \"gwint_name_ballista\", \"description\": \"gwint_desc_ballista\" }"; \
               } | jq -r '"\(.index) \(.title) \(.description)"')
               # The ballista cards aren't in the xml data files. Not sure where their source is
}

addCardsToMap
addMiscToMap
writePoFiles cards
printMissing

