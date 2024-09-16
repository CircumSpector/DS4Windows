#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/Vapour"
#$env:APPVEYOR_BUILD_VERSION = 1.0.1
#$env:PlATFORM = "x64"

$framework = "net8.0-windows10.0.17763.0"
$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish"
$output = ($env:APPVEYOR_BUILD_FOLDER + "/Publish/x64")

if (Test-Path -Path $publishFolder)
{
	Remove-Item -Path $publishFolder -Recurse
}

Write-Host "Starting dotnet build" -ForegroundColor Green

dotnet build ($env:APPVEYOR_BUILD_FOLDER + "/Vapour.sln") `
    --configuration Release `
    --framework $framework `
    --runtime win-$env:PLATFORM

Write-Host "Finished dotnet build" -ForegroundColor Green

Write-Host "Starting dotnet publish" -ForegroundColor Green

dotnet publish ($env:APPVEYOR_BUILD_FOLDER + "/Vapour.Client/Vapour.Client.csproj") `
    --no-build `
    --configuration Release `
    --framework $framework `
    --runtime win-$env:PLATFORM `
    --output $output `
    --self-contained false `
    --p:PublishSingleFile=false `
    --p:PublishReadyToRun=true

dotnet publish ($env:APPVEYOR_BUILD_FOLDER + "/Vapour.Client.TrayApplication/Vapour.Client.TrayApplication.csproj") `
    --no-build `
    --configuration Release `
    --framework $framework `
    --runtime win-$env:PLATFORM `
    --output $output `
    --self-contained false `
    --p:PublishSingleFile=false `
    --p:PublishReadyToRun=true

dotnet publish ($env:APPVEYOR_BUILD_FOLDER + "/Vapour.Server.Host/Vapour.Server.Host.csproj") `
    --no-build `
    --configuration Release `
    --framework $framework `
    --runtime win-$env:PLATFORM `
    --output $output `
    --self-contained false `
    --p:PublishSingleFile=false `
    --p:PublishReadyToRun=true

Write-Host "Finished dotnet publish" -ForegroundColor Green
