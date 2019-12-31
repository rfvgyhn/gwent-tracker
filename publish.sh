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
    version=$(get_xml "Version" ${projPath})
    dest="dist/${profile}"
    
    dotnet publish -c ${configuration} -f ${targetFramework} ${projPath} "/p:PublishProfile=${profile}"
    
    mkdir -p "${dest}" 
    tar -cvzf "${dest}/gwent-tracker_${version}_${runtime}.tar.gz" "${dest}/gwent-tracker"
}



publish linux
publish linux-standalone