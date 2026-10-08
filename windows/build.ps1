$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$Repo = Split-Path -Parent $Root
$Out = Join-Path $Repo "dist\BlinkMore-Windows"

$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
dotnet test (Join-Path $Root "BlinkMore.sln") -c Release
dotnet publish (Join-Path $Root "src\BlinkMore.App\BlinkMore.App.csproj") `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugType=none `
  -p:DebugSymbols=false `
  -o $Out

Copy-Item (Join-Path $Repo "LICENSE.md") (Join-Path $Out "LICENSE.md") -Force
Copy-Item (Join-Path $Root "third-party\NotoSansSC\OFL.txt") (Join-Path $Out "NotoSansSC-OFL.txt") -Force

$Zip = Join-Path $Repo "dist\BlinkMore-Windows-x64.zip"
if (Test-Path $Zip) { Remove-Item $Zip }
Compress-Archive -Path (Join-Path $Out "*") -DestinationPath $Zip
Copy-Item $Zip (Join-Path $Repo "BlinkMore-Windows-x64.zip") -Force
Write-Host "Published $Out\BlinkMore.exe"
