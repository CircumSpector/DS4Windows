#$env:APPVEYOR_BUILD_FOLDER = "c:/dev/DS4Windows"

$publishFolder = $env:APPVEYOR_BUILD_FOLDER + "/Publish"
$artifacts = $env:APPVEYOR_BUILD_FOLDER + "/artifacts"
$installerDirectory = $env:APPVEYOR_BUILD_FOLDER + "/Installer"
$installerProject = $installerDirectory + "/Ds4Windows.Installer.aip"
$installerOutputExe = $installerDirectory + "/Setup Files/DS4Windows.exe"
$installerOutputArtifact = $artifacts + "/DS4Windows.Setup.exe"
$publishOutputArtifact = $artifacts + "/Publish.zip"
$adi = "C:/Program Files (x86)/Caphyon/Advanced Installer 19.5/bin/x86/AdvancedInstaller.com"

if (Test-Path -Path $artifacts)
{
	Remove-Item -Path $artifacts -Recurse
}

& "$adi" /register a154fa587445df371a718f08290b7c2b
& "$adi" /build $installerProject 

New-Item $artifacts -itemtype directory
Compress-Archive -Path $publishFolder -DestinationPath $publishOutputArtifact
Move-Item -Path $installerOutputExe -Destination $installerOutputArtifact