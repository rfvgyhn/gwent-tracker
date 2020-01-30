#!/bin/bash

set -e

strings="/mnt/secondary/witcher3/extracted-data"
miscMapFile="localizationkeys.txt"
localDir="../src/GwentTracker/locale"
declare -A stringMap
missing=()

function addToMap() {
    local id=$(echo "$1" | cut -d '|' -f 1)
    local value=$(echo "$1" | cut -d '|' -f 2)
    stringMap[$value]="$id"
}

function addMiscToMap() {    
    echo "Adding misc to map..."
    while read line; do
        addToMap "$line"
    done <<< $(grep '|' "$miscMapFile")
}

function cleanStr() {
    [[ ${1} -eq true ]] && br=": " || br=" "
    
    # Input is piped to this function which bash automatically passes to first command
    sed "s/<br>/${br}/g" |\
    sed "s/’/'/g" |\
    sed 's/[“”]/"/g' |\
    sed 's/"/\\"/g' |\
    sed 's:</\?[a-z]*>\|^ *::g'
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
    
    grep -r --include "${locale}.w3strings.csv" " ${id}" "${strings}" | \
    tail -n1 | \
    cut -d '|' -f 4 | \
    sed 's/^\s+//'
}

function printMissing() {
    echo "Missing Translations: ${#missing[@]}"
    printf '%s\n' "${missing[@]}"
    echo "Missing Translations: ${#missing[@]}"
}

function writePoFiles() {
    for locale in ${locales[@]}; do
        local src="${localDir}/$1.pot"
        local dest="${localDir}/$1.${locale}.po"
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
        while read -r line; do
            [[ "$line" =~ ^msgstr ]] && continue
            echo "$line" >> ${tempDest}        
            
            if [[ "$line" =~ $re ]]; then
                local id=${BASH_REMATCH[1]}
                
                [[ "$id" == "" ]] && continue
                
                local preparedId=$(prepareId "$id")
                
                if [[ ! ${stringMap[$id]+_} ]]; then
                    local existingTranslation=$([[ -f "${dest}" ]] && sed -n "/^msgid \"$preparedId\"$/{n;p;}" "${dest}" | sed -r 's/^msgstr "(.+)"$/\1/' || echo 'msgstr ""')
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