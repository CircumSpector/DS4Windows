rm -r -fo .\Publish

dotnet publish ".\DS4Windows.Client\DS4Windows.Client.csproj" /p:PublishProfile=release-win-x64
dotnet publish ".\DS4Windows.Client\DS4Windows.Client.csproj" /p:PublishProfile=release-win-x86
dotnet build ".\DS4Windows.Server.Host\DS4Windows.Server.Host.csproj" /p:DeployOnBuild=true /p:PublishProfile=release-win-x64
dotnet build ".\DS4Windows.Server.Host\DS4Windows.Server.Host.csproj" /p:DeployOnBuild=true /p:PublishProfile=release-win-x86
dotnet publish ".\DS4Windows.Client.TrayApplication\DS4Windows.Client.TrayApplication.csproj" /p:PublishProfile=release-win-x64
dotnet publish ".\DS4Windows.Client.TrayApplication\DS4Windows.Client.TrayApplication.csproj" /p:PublishProfile=release-win-x86

 ls .\Publish\*.pdb -Recurse | foreach {rm $_}