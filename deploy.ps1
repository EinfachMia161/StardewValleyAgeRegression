# AgeRegression Development Deployment Script
# Builds and deploys the mod to Stardew Valley's Mods folder

param(
    [string]$Destination = "C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\AgeRegression"
)

Write-Host "Building AgeRegression in Release mode..." -ForegroundColor Cyan
dotnet build AgeRegression\AgeRegression.csproj -c Release 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Aborting deployment." -ForegroundColor Red
    exit 1
}

Write-Host "Build successful." -ForegroundColor Green

# Ensure destination directory exists
$null = New-Item -ItemType Directory -Path $Destination -Force

# Copy required files
$sourceRoot = "AgeRegression"
$binPath = Join-Path $sourceRoot "bin\Release\net6.0"

# Copy DLL
$dllSource = Join-Path $binPath "AgeRegression.dll"
$dllDest = Join-Path $Destination "AgeRegression.dll"
if (Test-Path $dllSource) {
    Copy-Item -LiteralPath $dllSource -Destination $dllDest -Force
    Write-Host "Copied: AgeRegression.dll" -ForegroundColor Yellow
}

# Copy manifest
$manifestSource = Join-Path $sourceRoot "manifest.json"
$manifestDest = Join-Path $Destination "manifest.json"
if (Test-Path $manifestSource) {
    Copy-Item -LiteralPath $manifestSource -Destination $manifestDest -Force
    Write-Host "Copied: manifest.json" -ForegroundColor Yellow
}

# Copy i18n folder if exists
$i18nSource = Join-Path $sourceRoot "i18n"
$i18nDest = Join-Path $Destination "i18n"
if (Test-Path $i18nSource) {
    Copy-Item -LiteralPath $i18nSource -Destination $i18nDest -Recurse -Force
    Write-Host "Copied: i18n/" -ForegroundColor Yellow
}

# Copy assets folder
$assetsSource = Join-Path $sourceRoot "assets"
$assetsDest = Join-Path $Destination "assets"
if (Test-Path $assetsSource) {
    Copy-Item -LiteralPath $assetsSource -Destination $assetsDest -Recurse -Force
    Write-Host "Copied: assets/" -ForegroundColor Yellow
}

# Copy config schema if exists
$configSource = Join-Path $sourceRoot "config-schema.json"
$configDest = Join-Path $Destination "config-schema.json"
if (Test-Path $configSource) {
    Copy-Item -LiteralPath $configSource -Destination $configDest -Force
    Write-Host "Copied: config-schema.json" -ForegroundColor Yellow
}

Write-Host "Deployment complete: $Destination" -ForegroundColor Green