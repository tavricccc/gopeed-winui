param([switch]$Test)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$env:CGO_ENABLED = '0'
Push-Location (Join-Path $repo 'core')
try {
    go build -trimpath -ldflags '-s -w -H=windowsgui' -o gopeed-core.exe .
    if ($LASTEXITCODE) { throw 'Go core build failed' }
    if ($Test) { go test .; if ($LASTEXITCODE) { throw 'Core verification failed' } }
} finally { Pop-Location }
if ($Test) {
    dotnet run --project (Join-Path $repo 'tests/Gopeed.ProtocolChecks/ProtocolCheck.csproj')
    if ($LASTEXITCODE) { throw 'Gopeed protocol verification failed' }
}
$portable = [IO.Path]::GetFullPath((Join-Path $repo 'artifacts/portable'))
if ($portable -ne "$repo\artifacts\portable") { throw 'Unexpected publish path' }
if (Test-Path -LiteralPath $portable) { Remove-Item -LiteralPath $portable -Recurse -Force }
dotnet publish (Join-Path $repo 'src/Gopeed.Native/Gopeed.Native.csproj') -c Release -r win-x64 -p:Platform=x64 -o $portable
if ($LASTEXITCODE) { throw 'WinUI publish failed' }
