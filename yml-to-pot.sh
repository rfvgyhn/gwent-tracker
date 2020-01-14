#!/usr/bin/env bash

source="src/GwentTracker/data"
dest="src/GwentTracker/locale/cards.pot"
now=$(date +"%Y-%m-%d %H:%M%z")
cat > ${dest} <<EOF
msgid ""
msgstr ""
"POT-Creation-Date: ${now}\n"
"PO-Revision-Date: ${now}\n"
"Plural-Forms: nplurals=2; plural=n != 1;\n"
"X-Poedit-KeywordsList: GetString;GetPluralString:1,2;GetParticularString:1c,2;GetParticularPluralString:1c,2,3;_;_n:1,2;_p:1c,2;_pn:1c,2,3\n"
EOF

egrep -no '\w+:\ "?[A-Za-z_ ]+"?$' ${source}/*.yml | \
 sed 's/"//g' | \
 cut -d ':' -f 1-4 --output-delimiter='|' | \
 sort -t'|' -uk3 | \
 sort -t'|' -k1,1 -k2n,2 | \
 sed -e 's/|\s/|/g' | \
 while read line; do
    IFS='|' read -ra parts <<< "$line"
    file="${parts[0]}"
    lineNum="${parts[1]}"
    property="${parts[2]}"
    value="${parts[3]}"
    
    cat >> ${dest} <<EOF
    
#: ${file}:${lineNum} -> Card.${property}
msgid "${value}"
msgstr ""
EOF
 done