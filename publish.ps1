$projPath = "src\GwentTracker\GwentTracker.csproj"
$proj = [xml](Get-Content $projPath)
$profile = [xml](Get-Content src\GwentTracker\Properties\PublishProfiles\windows.pubxml)
$configuration = $profile.Project.PropertyGroup.Configuration
$targetFramework = $profile.Project.PropertyGroup.TargetFramework
$runtime = $profile.Project.PropertyGroup.RuntimeIdentifier
$version = $proj.Project.PropertyGroup.Version

Remove-Item "dist\windows\" -Recurse

dotnet publish -c $configuration -f $targetFramework $projPath /p:PublishProfile=windows

pushd "dist\windows\"

try {
    $source = "gwent-tracker_${version}_$runtime"
    Rename-Item -Path "gwent-tracker" -NewName $source
    
    $dest = $source + "\lib"
    $filter = $source + "\*.dll"
        
    if (!(Test-Path $dest)) {
        New-Item -Path $dest -ItemType Directory -Force
    }
    get-childitem -Path $filter | move-item -destination $dest
    compress-archive -force -path "$source" -destinationpath "$source.zip"
}
finally {
    popd
}