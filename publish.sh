#!/usr/bin/env sh

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
    version=$(get_xml "Version" ${projPath})
    selfContained=$(get_xml "SelfContained" ${profilePath})
    compressedName="${folderName}_${version}_${runtime}"
    
    dotnet publish -c ${configuration} -f ${targetFramework} ${projPath} "/p:PublishProfile=${profile}"
    sed -i "2s#.*#defaultSavePath=~/.local/share/Steam/steamapps/compatdata/292030/pfx/drive_c/users/steamuser/My Documents/The Witcher 3/gamesaves/#" "${publishDir}/settings.ini"
    tar -C "${publishDir}" -cvzf "${publishDir}/../${compressedName}.tar.gz" --transform="s/^\./${compressedName}/g" .
}



publish linux
publish linux-selfcontained