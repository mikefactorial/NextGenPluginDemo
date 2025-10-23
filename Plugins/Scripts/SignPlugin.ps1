# 4. Sign the plug-in assembly with the certificate. The script below needs to be run anytime the plugin is rebuilt
# Note: The signtool utility is part of the Windows SDK (Software Development Kit). You can find it in the installation directory of the Windows SDK. If you haven't already installed the Windows SDK, you can download it from here: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/
$certificatePath = "C:\source\repos\NextGenPluginDemo\certificate.pfx"
$password = "<certificate_password>"
$fileDigestAlgorithm = "SHA256"
$signToolPath = "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe"

dotnet nuget sign "C:\source\repos\NextGenPluginDemo\bin\Debug\NextGenDemo.Plugins.1.0.0.nupkg" --certificate-path "C:\source\repos\NextGenPluginDemo\certificate.pfx" --certificate-password "certificate_password" --overwrite