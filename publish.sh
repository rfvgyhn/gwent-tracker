#!/bin/bash

set -e

get_xml() {
    grep "<$1" $2 | cut -f2 -d">" | cut -f1 -d"<"
}

publish() {
    profile=$1
    projPath=src/GwentTracker/GwentTracker.csproj
    profilePath="src/GwentTracker/Properties/PublishProfiles/${profile}.pubxml"
    configuration=$(get_xml "Configuration" ${profilePath})
    targetFramework=$(get_xml "TargetFramework" ${profilePath})
    runtime=$(get_xml "RuntimeIdentifier" ${profilePath})
    publishDir=$(get_xml "PublishDir" ${profilePath} | awk -F'\\.\\./' '{print $NF}')
    folderName=$(get_xml "PublishDir" ${profilePath} | xargs basename)
    publishParentDir=$(echo "${publishDir}" | sed "s:/${folderName}$::")
    version=$(get_xml "Version" ${projPath})
    selfContained=$(get_xml "SelfContained" ${profilePath})
    compressedName="${folderName}_${version}_${runtime}"
    
    dotnet publish -c ${configuration} -f ${targetFramework} ${projPath} "/p:PublishProfile=${profile}"
    ln -s "$(pwd)/${publishDir}" "$(pwd)/${publishParentDir}/${compressedName}"
    
    pushd "${publishParentDir}"    
        case "$profile" in
            linux*)
                sed -i "2s#.*#defaultSavePath=~/.local/share/Steam/steamapps/compatdata/292030/pfx/drive_c/users/steamuser/My Documents/The Witcher 3/gamesaves/#" "${folderName}/settings.ini"
                tar -C "${compressedName}" -cvzf "${compressedName}.tar.gz" --transform="s/^\./${compressedName}/g" .
                ;;
            windows)
                mkdir -p "${folderName}/lib"
                mv "${folderName}"/*.dll "${folderName}/lib"
                ;;
        esac
        
        zip -r -b "${XDG_RUNTIME_DIR:-/tmp}" "${compressedName}.zip" "${compressedName}"
    popd
    
    rm "${publishParentDir}/${compressedName}"
}

rm -r dist/
publish linux
publish linux-selfcontained
publish windows
