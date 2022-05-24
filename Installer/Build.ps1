#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/DS4Windows"

$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish"
$clientProject = $env:APPVEYOR_BUILD_FOLDER + "/DS4Windows.Client/DS4Windows.Client.csproj"
$serverProject = $env:APPVEYOR_BUILD_FOLDER + "/DS4Windows.Server.Host/DS4Windows.Server.Host.csproj"
$trayProject = $env:APPVEYOR_BUILD_FOLDER + "/DS4Windows.Client.TrayApplication/DS4Windows.Client.TrayApplication.csproj"

if (Test-Path -Path $publishFolder)
{
	Remove-Item -Path $publishFolder -Recurse
}

dotnet publish $clientProject /p:PublishProfile=release-win-x64
dotnet publish $clientProject /p:PublishProfile=release-win-x86
dotnet build $serverProject /p:DeployOnBuild=true /p:PublishProfile=release-win-x64
dotnet build $serverProject /p:DeployOnBuild=true /p:PublishProfile=release-win-x86
dotnet publish $trayProject /p:PublishProfile=release-win-x64
dotnet publish $trayProject /p:PublishProfile=release-win-x86