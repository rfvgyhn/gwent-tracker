$projPath = "src\GwentTracker\GwentTracker.csproj"
$proj = [xml](Get-Content $projPath)
$profile = [xml](Get-Content src\GwentTracker\Properties\PublishProfiles\windows.pubxml)
$configuration = $profile.Project.PropertyGroup.Configuration
$targetFramework = $profile.Project.PropertyGroup.TargetFramework
$version = $proj.Project.PropertyGroup.Version

dotnet publish -c $configuration -f $targetFramework $projPath /p:PublishProfile=windows

pushd "dist\windows\"

try {
    $source = "gwent-tracker\"
    $dest = $source + "lib"
    $filter = $source + "*.dll"
        
    if (!(Test-Path $dest)) {
        New-Item -Path $dest -ItemType Directory -Force
    }
    get-childitem -Path $filter | move-item -destination $dest
    compress-archive -force -path "$source*" -destinationpath "gwent-tracker_$version_$targetFramework.zip"
}
finally {
    popd
}