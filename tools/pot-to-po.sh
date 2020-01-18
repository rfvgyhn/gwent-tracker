#!/bin/bash

set -e

# Depends on:
#    jq - https://stedolan.github.io/jq/
#    xml-to-json - https://github.com/sinelaw/xml-to-json

scriptName=$(basename "$0")
data="/mnt/secondary/witcher3/uncooked-data/base/gameplay/items"
strings="/mnt/secondary/witcher3/extracted-data"
localDir="../src/GwentTracker/locale"
miscMapFile="localizationkeys.txt"
hashMap="card-id-hash-map.txt"
declare -A stringMap
missing=()

if [[ ${#@} = 0 ]]; then
    locales=("ar" "cs" "de" "es" "es-mx" "fr" "hu" "it" "ja" "ko" "pl" "pt-br" "ru" "tr" "zh" "zh-cn")
else
    locales=($@)
fi

function jqstr() {
   echo ".redxml.custom.gwint_${1}.card[]"
}

function getHashFromId() {
    grep "$1$" "$hashMap" | head -n1 | cut -d '|' -f 1
}

function cleanStr() {
    # Input is piped to this function which bash automatically passes to first command
    sed "s/<br>/ /g" |\
    sed "s/’/'/g" |\
    sed 's:</\?[a-z]*>\|["“”]\|^ *::g'
}

function getEnglishString() {
    find "$strings" -name "en*.csv" |\
    xargs -n1 grep "${1}" 2>/dev/null |\
    head -n1 |\
    cut -d '|' -f 1,4 |\
    cleanStr
}

function addToMap() {
    local id=$(echo "$1" | cut -d '|' -f 1)
    local value=$(echo "$1" | cut -d '|' -f 2)
    stringMap[$value]="$id"
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
        
        addToMap "$(getEnglishString ${titleHash})"
        addToMap "$(getEnglishString ${descHash})"
    done <<< $({ xml-to-json "${data}/def_gwint_battle_king_cards.xml" | jq -r "$(jqstr battle_king_card_definitions)"; \
                 xml-to-json "${data}/def_gwint_cards_final.xml" | jq "$(jqstr card_definitions_final)"; \
               } | jq -r '"\(.index) \(.title) \(.description)"')
}

function addMiscToMap() {    
    echo "Adding misc to map..."
    while read line; do
        addToMap "$line"
    done <<< $(grep '|' "$miscMapFile")
}

function getTranslation() {
    local id="$1"
    local locale="$2"
    
    case "$locale" in
        cs) locale="cz";;
        es-mx) locale="esMX";;
        ja) locale="jp";;
        ko) locale="kr";;
        pt-br) locale="br";;
        zh-cn) locale="cn";;
    esac
    
    grep -r --include "${locale}.w3strings.csv" "${id}" "${strings}" | \
    head -n1 | \
    cut -d '|' -f 4
}

function writePoFiles() {
    for locale in ${locales[@]}; do
        local src="${localDir}/cards.pot"
        local dest="${localDir}/cards.${locale}.po"
        local tempDest=$(mktemp -t "$scriptName.XXXXXXXXXX" -p "${XDG_RUNTIME_DIR:-/tmp}")
        
        echo "Generating ${dest}"
        
        cat > ${tempDest} <<EOF
## 'msgid's in this file come from POT (.pot) files.
##
## Do not add, change, or remove 'msgid's manually here as
## they're tied to the ones in the corresponding POT file
## (with the same domain).
msgid ""
msgstr ""
"Language: ${locale}\n"
"Content-Type: text/plain; charset=UTF-8\n"
"Plural-Forms: nplurals=2; plural=n != 1;\n"
"X-Poedit-KeywordsList: GetString;GetPluralString:1,2;GetParticularString:1c,2;GetParticularPluralString:1c,2,3;_;_n:1,2;_p:1c,2;_pn:1c,2,3\n"

EOF
        local re='^msgid "(.*)"$'
        while read line; do
            [[ "$line" =~ ^msgstr ]] && continue
            echo "$line" >> ${tempDest}        
            
            if [[ "$line" =~ $re ]]; then
                local id=${BASH_REMATCH[1]}
                
                [[ "$id" == "" ]] && continue
                if [[ ! ${stringMap[$id]+_} ]]; then
                    local existingTranslation=$([[ -f "${dest}" ]] && sed -n "/^msgid \"$id\"$/{n;p;}" "${dest}" | sed -r 's/^msgstr "(.+)\"$/\1/' || echo 'msgstr ""')
                    if [[ "${existingTranslation}" == 'msgstr ""' ]]; then
                        if ! printf '%s\n' ${missing[@]} | grep -q -P "^${locale} - ${id}$"; then
                            missing+=("${locale} - ${id}")
                        fi
                        echo "${existingTranslation}" >> ${tempDest}
                    else
                        echo "msgstr \"${existingTranslation}\"" >> ${tempDest}
                    fi
                else
                    local translationId="${stringMap["$id"]}"
                    local translation=$(getTranslation "$translationId" "$locale" | cleanStr)
                    echo "msgstr \"${translation}\"" >> ${tempDest}  
                fi
            fi
        done < <(tail -n +6 "$src")
        
        mv "${tempDest}" "${dest}"
    done
}

addCardsToMap
addMiscToMap
writePoFiles

echo "Missing Translations: ${#missing[@]}"
printf '%s\n' "${missing[@]}"
echo "Missing Translations: ${#missing[@]}"

