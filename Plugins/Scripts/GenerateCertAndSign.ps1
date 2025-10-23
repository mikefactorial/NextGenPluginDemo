# Managed Identity Federated Credentials
# Credit: https://medium.com/rapha%C3%ABl-pothin/power-platforms-protection-managed-identity-for-dataverse-plug-ins-0ae0ed405338
# Issuer: https://environment_prefix.environment_suffix.environment.api.powerplatform.com/sts
# Audience: api://azureadtokenexchange
# Name: NextGenPluginDemo
# Subject Identifier: component:pluginassembly,thumbprint:cert_thumbprint_all_caps,environment:environment_id
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
$certificatePath = "C:\source\repos\NextGenPluginDemo\certificate.pfx"
$password = "<certificate_password>"
$name = "NextGenPluginDemo"
$friendlyName = "Next Gen Plugins Demo"
$fileDigestAlgorithm = "SHA256"
$cert = New-SelfSignedCertificate -Subject "CN=$name, O=corp, C=$name.com" -DnsName "www.$name.com" -Type CodeSigning -KeyUsage DigitalSignature -CertStoreLocation Cert:\CurrentUser\My -FriendlyName $friendlyName
$signToolPath = "C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe"

# Note: The cert object contains a Thumbprint property we will use for the configuration of the federated credentials of the managed identity so keep it available

# 2. Set a password for the private key (optional)
$pw = ConvertTo-SecureString -String $password -Force -AsPlainText

# 3. Export the certificate as a PFX file
Export-PfxCertificate -Cert $cert -FilePath $certificatePath -Password $pw

# 4. Sign the NuGet package with the certificate
dotnet nuget sign "C:\source\repos\NextGenPluginDemo\bin\Debug\NextGenDemo.Plugins.1.0.0.nupkg" --certificate-path "C:\source\repos\NextGenPluginDemo\certificate.pfx" --certificate-password "certificate_password" --overwrite