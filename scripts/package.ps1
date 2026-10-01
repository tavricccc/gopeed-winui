$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
& (Join-Path $PSScriptRoot 'build.ps1')
$compiler = Join-Path $env:LOCALAPPDATA 'Programs/Inno Setup 6/ISCC.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = (Get-Command ISCC.exe -ErrorAction Stop).Source }
& $compiler (Join-Path $PSScriptRoot 'installer.iss')
if ($LASTEXITCODE) { throw 'Installer compilation failed' }
Compress-Archive -Path (Join-Path $repo 'artifacts/portable/*') -DestinationPath (Join-Path $repo 'artifacts/GopeedNative-Portable-0.2.0-x64.zip') -Force
