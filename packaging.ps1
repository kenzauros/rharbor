$appName = "RHarbor"
$currentDir = Split-Path $MyInvocation.MyCommand.Path -Parent
$directory = Join-Path $currentDir "RHarbor\bin" -Resolve
$releaseDir = Join-Path $directory "Release" -Resolve
$filename = $appName + ".exe"
$exePath = Join-Path $releaseDir $filename -Resolve
$fileVersion = "v" + ((Get-ItemProperty $exePath).VersionInfo.FileVersion)
$version = &"git" "describe" "--tags"
$destDir = Join-Path $directory ($appName + "_" + $version)
$destPath = $destDir + ".zip"

Write-Output "Current dir.: $currentDir"
Write-Output "Bin dir.: $directory"
Write-Output "Release dir.: $releaseDir"
Write-Output "Version (exe file): $fileVersion"
Write-Output "Version (from git tag): $version"
Write-Output "Destination dir.: $destDir"
Write-Output "Output file: $destPath"

Write-Output "Creating destination Directory..."

# Create destination Directory
New-Item $destDir -ItemType Directory -Force | Out-Null
Get-ChildItem $destDir | Remove-Item -Recurse -Force

Write-Output "Copying files..."

# Copy READMEs in the current dir.
Get-ChildItem $currentDir -Filter "README.*" | Copy-Item -Destination $destDir -Recurse -Force
# Copy directories under the Release dir.
Get-ChildItem $releaseDir -Directory | Copy-Item -Destination $destDir -Recurse -Force
# Copy files in the Release dir.
Get-ChildItem $releaseDir | Where-Object Name "\.(exe|dll|json|config)$" -Match | Copy-Item -Destination $destDir

Write-Output "Compressing..."

# Compress
Compress-Archive -Path $destDir -DestinationPath $destPath -Force

Write-Output "Successfully completed."
