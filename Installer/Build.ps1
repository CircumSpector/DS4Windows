#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/DS4Windows"
#$env:APPVEYOR_BUILD_VERSION = 1.0.1
#$env:PlATFORM = "x64"

$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish"

if (Test-Path -Path $publishFolder)
{
	Remove-Item -Path $publishFolder -Recurse
}

Write-Host "Starting dotnet publish" -ForegroundColor Green

dotnet publish ($env:APPVEYOR_BUILD_FOLDER + "/Vapour.sln") /p:PlatformTarget=$env:PLATFORM /p:PublishProfile=release-win-$env:PLATFORM

Write-Host "Finished dotnet publish" -ForegroundColor Green
