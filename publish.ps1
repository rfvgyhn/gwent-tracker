$ErrorActionPreference = "Stop"

$target="win-x64"
[xml]$proj = Get-Content src/GwentTracker/GwentTracker.csproj
[string]$version=$proj.Project.PropertyGroup.VersionPrefix
$version=$version.Trim()
$release_name="gwent-tracker_v${version}_${target}"
$target_dir="artifacts\$release_name"

dotnet publish -r "$target" --self-contained -o "$target_dir" -c Release -p:PublishSingleFile=true src/GwentTracker/GwentTracker.csproj
cp readme.md,changelog.md -Destination "$target_dir" -Force
rm "$target_dir\*" -include *.json, *.pdb

Compress-Archive -Path "$target_dir" -DestinationPath "$target_dir.zip" -Force

rm -r "$target_dir"