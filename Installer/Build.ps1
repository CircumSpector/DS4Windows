#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/Vapour"
#$env:APPVEYOR_BUILD_VERSION = 1.0.1
#$env:PlATFORM = "x64"

$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish"

if (Test-Path -Path $publishFolder)
{
	Remove-Item -Path $publishFolder -Recurse
}

Write-Host "Starting dotnet publish" -ForegroundColor Green

dotnet publish ($env:APPVEYOR_BUILD_FOLDER + "/Vapour.sln") `
    --configuration Release `
    --framework net8.0-windows10.0.17763.0 `
    --runtime win-$env:PLATFORM `
    --output ($env:APPVEYOR_BUILD_FOLDER + "/Publish/x64") `
    --self-contained false `
    --p:PublishSingleFile=false `
    --p:PublishReadyToRun=true

Write-Host "Finished dotnet publish" -ForegroundColor Green
