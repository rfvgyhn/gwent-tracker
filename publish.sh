#!/bin/bash

set -e

publish() {
    target=$1
    version=$(grep -oPm1 "(?<=<VersionPrefix>)[^<]+" src/GwentTracker/GwentTracker.csproj) # use something like xml_grep if this regex becomes a problem
    release_name="gwent-tracker_v${version}_$target"
    icon=$(readlink -f src/GwentTracker/Assets/collector.ico)
    rcedit=$(readlink -f tools/rcedit-x64.exe)
    
    pushd "tools"
        ./compile-po.sh
    popd
    
    dotnet restore -r $target
    dotnet publish src/GwentTracker/GwentTracker.csproj -r "$target" --self-contained true --no-restore -o "artifacts/$release_name" -c Release -p:PublishSingleFile=true
    cp readme.md changelog.md "artifacts/$release_name"
    rm artifacts/"$release_name"/*.pdb
    
    case "$target" in
        lin*)
            sed -i "s:^defaultSavePath.*$:defaultSavePath=~/.local/share/Steam/steamapps/compatdata/292030/pfx/drive_c/users/steamuser/My Documents/The Witcher 3/gamesaves/:" "artifacts/"$release_name"/settings.ini"
            tar czvf "artifacts/$release_name.tar.gz" -C "artifacts" "$release_name"
            ;;
        win*)            
            # Workaround for https://github.com/dotnet/sdk/issues/3943
            WINEDEBUG=fixme-all wine "${rcedit}" "artifacts/$release_name/GwentTracker.exe" --set-icon "${icon}"
            (cd "artifacts/$release_name" && zip -qr -b "${XDG_RUNTIME_DIR:-/tmp}" - .) > "artifacts/$release_name.zip"
            ;;
    esac
    
    rm -r "artifacts/$release_name"
}

publish "linux-x64"
publish "win-x64"
