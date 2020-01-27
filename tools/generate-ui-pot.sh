#!/bin/bash

domain=gwent-tracker
dest=../src/GwentTracker/locale/${domain}.pot
cardsPot=../src/GwentTracker/locale/cards.pot
sourceFiles=../src/GwentTracker/ViewModels/*.cs

cat > ${dest} <<EOF
msgid ""
msgstr ""
"Content-Type: text/plain; charset=UTF-8\n"
"Plural-Forms: nplurals=2; plural=n != 1;\n"
"X-Poedit-KeywordsList: GetString;GetPluralString:1,2;GetParticularString:1c,2;GetParticularPluralString:1c,2,3;_;_n:1,2;_p:1c,2;_pn:1c,2,3\n"

EOF

egrep -no '_t\[\$?"([a-zA-Z {}]+)"\]' ${sourceFiles} | \
 sed -r 's/_t\[\$?"(.+)"\]/\1/' | \
 sed 's/:/`/1;s//`/1' | \
 cut -d '`' -f 1-3 | \
 sort -t '`' -u -k 3,3 | \
 sort -t '`' -k1,1 -k2n,2 | \
 while read line; do
    IFS='`' read -r file lineNum value <<< "$line"
    grep -q "msgid \"$value\"" "${cardsPot}" && continue
    
    cat >> ${dest} <<EOF
#: ${file:3}:${lineNum}
msgid "${value}"
msgstr ""

EOF
 done

csi XamlToPot.csx ../src/GwentTracker/Views/MainWindow.xaml >> ${dest}
