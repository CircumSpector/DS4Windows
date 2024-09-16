#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/Vapour"
#$env:APPVEYOR_BUILD_VERSION = 1.0.1
#$env:PlATFORM = "x64"

$platform = $env:PLATFORM
$framework = "net8.0-windows10.0.17763.0"
$output = ($env:APPVEYOR_BUILD_FOLDER + "/Publish/x64")
$projectRoot = $env:APPVEYOR_BUILD_FOLDER
$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish"

if (Test-Path -Path $publishFolder)
{
	Remove-Item -Path $publishFolder -Recurse
}

Write-Host "Starting dotnet publish" -ForegroundColor Green

dotnet publish ($projectRoot + "/Vapour.Client/Vapour.Client.csproj") `
    --configuration Release `
    --framework $framework `
    --runtime win-$platform `
    --output $output `
    --self-contained false `
    --p:PublishSingleFile=false

dotnet publish ($projectRoot + "/Vapour.Client.TrayApplication/Vapour.Client.TrayApplication.csproj") `
    --configuration Release `
    --framework $framework `
    --runtime win-$platform `
    --output $output `
    --self-contained false `
    --p:PublishSingleFile=false

dotnet publish ($projectRoot + "/Vapour.Server.Host/Vapour.Server.Host.csproj") `
    --configuration Release `
    --framework $framework `
    --runtime win-$platform `
    --output $output `
    --self-contained false `
    --p:PublishSingleFile=false

Write-Host "Finished dotnet publish" -ForegroundColor Green
