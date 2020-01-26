#!/bin/bash

set -e

scriptName=$(basename "$0")
localDir="../src/GwentTracker/locale"
declare -A stringMap
missing=()

if [[ ${#@} = 0 ]]; then
    locales=("ar" "cs" "de" "es" "es-mx" "fr" "hu" "it" "ja" "ko" "pl" "pt-br" "ru" "tr" "zh" "zh-cn")
else
    locales=($@)
fi



function writePoFiles() {
    for locale in ${locales[@]}; do
        local src="${localDir}/gwent-tracker.pot"
        local dest="${localDir}/gwent-tracker.${locale}.po"
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
                
                local escapedId=$(echo "$id" | sed 's/\([*]\)/\\\1/g')
                local existingTranslation=$([[ -f "${dest}" ]] && sed -n "/^msgid \"$escapedId\"$/{n;p;}" "${dest}" | sed -r 's/^msgstr "(.+)"$/\1/' || echo 'msgstr ""')
                if [[ "${existingTranslation}" == 'msgstr ""' ]]; then
                    if ! printf '%s\n' ${missing[@]} | grep -q -P "^${locale} - ${id}$"; then
                        missing+=("${locale} - ${id}")
                    fi
                    echo "${existingTranslation}" >> ${tempDest}
                else
                    echo "msgstr \"${existingTranslation}\"" >> ${tempDest}
                fi 
                
            fi
        done < <(tail -n +6 "$src")
        
        mv "${tempDest}" "${dest}"
    done
}

writePoFiles

echo "Missing Translations: ${#missing[@]}"
printf '%s\n' "${missing[@]}"
echo "Missing Translations: ${#missing[@]}"

