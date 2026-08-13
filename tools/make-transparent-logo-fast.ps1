Add-Type -TypeDefinition (Get-Content "tools\TransparentLogo.cs" -Raw) -ReferencedAssemblies System.Drawing

$tempLogo = Join-Path $env:TEMP "dil_khan_logo_transparent.png"
[TransparentLogo]::MakeTransparent((Resolve-Path "img\logo.png").Path, $tempLogo, 235)

Copy-Item -LiteralPath $tempLogo -Destination "img\logo-transparent.png" -Force
Copy-Item -LiteralPath $tempLogo -Destination "img\footer_logo-transparent.png" -Force
Remove-Item $tempLogo -Force
