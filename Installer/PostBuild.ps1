#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/DS4Windows"
#$env:APPVEYOR_BUILD_VERSION = 1.0.1
#$env:PlATFORM = "x64"

$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish/" + $env:PlATFORM
$artifacts = $env:APPVEYOR_BUILD_FOLDER + "/artifacts"
$installerDirectory = $env:APPVEYOR_BUILD_FOLDER + "/Installer"
$installerProject = $installerDirectory + "/Ds4Windows.Installer." + $env:PlATFORM + ".aip"
$installerOutputExe = $installerDirectory + "/Setup Files " + $env:PlATFORM + "/DS4Windows Setup (" + $env:PlATFORM + ").exe"
$installerOutputArtifact = $artifacts + "/DS4Windows Setup (" + $env:PlATFORM + ").exe"
$publishOutputArtifact = $artifacts + "/Publish.zip"
$adi = "C:/Program Files (x86)/Caphyon/Advanced Installer 19.8.1/bin/x86/AdvancedInstaller.com"
$serial = $installerDirectory + "/AppVeyor_ProductKey.txt"

if (Test-Path -Path $artifacts)
{
	Remove-Item -Path $artifacts -Recurse
}

& "$adi" /register $(gc $serial)
& "$adi" /edit $installerProject /SetVersion $env:APPVEYOR_BUILD_VERSION
& "$adi" /build $installerProject 

New-Item $artifacts -itemtype directory
Compress-Archive -Path $publishFolder -DestinationPath $publishOutputArtifact
Move-Item -Path $installerOutputExe -Destination $installerOutputArtifact