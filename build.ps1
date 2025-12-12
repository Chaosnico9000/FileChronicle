#!/usr/bin/env pwsh
# Build script for FileChronicle

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter(Mandatory=$false)]
    [switch]$Clean,
    
    [Parameter(Mandatory=$false)]
    [switch]$Publish,
    
    [Parameter(Mandatory=$false)]
    [switch]$Pack,
    
    [Parameter(Mandatory=$false)]
    [string]$Runtime = ''
)

$ErrorActionPreference = 'Stop'

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  FileChronicle Build Script                                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$ProjectFile = "FileChronicle.csproj"
$PublishDir = "publish"

# Clean
if ($Clean) {
    Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    if (Test-Path $PublishDir) { Remove-Item -Recurse -Force $PublishDir }
    Write-Host "✅ Clean complete" -ForegroundColor Green
    Write-Host ""
}

# Restore
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "✅ Restore complete" -ForegroundColor Green
Write-Host ""

# Build
Write-Host "🔨 Building project ($Configuration)..." -ForegroundColor Yellow
dotnet build $ProjectFile --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "✅ Build complete" -ForegroundColor Green
Write-Host ""

# Publish
if ($Publish) {
    Write-Host "📤 Publishing..." -ForegroundColor Yellow
    
    $runtimes = @()
    
    if ($Runtime) {
        $runtimes = @($Runtime)
    } else {
        # Publish for common platforms
        $runtimes = @(
            'win-x64',
            'win-arm64',
            'linux-x64',
            'linux-arm64',
            'osx-x64',
            'osx-arm64'
        )
    }
    
    foreach ($rid in $runtimes) {
        Write-Host "  Publishing for $rid..." -ForegroundColor Cyan
        
        $outputPath = Join-Path $PublishDir $rid
        
        dotnet publish $ProjectFile `
            --configuration $Configuration `
            --runtime $rid `
            --self-contained true `
            --output $outputPath `
            /p:PublishSingleFile=true `
            /p:IncludeNativeLibrariesForSelfExtract=true
        
        if ($LASTEXITCODE -ne 0) { 
            Write-Host "  ❌ Failed to publish for $rid" -ForegroundColor Red
            exit $LASTEXITCODE 
        }
        
        Write-Host "  ✅ Published to $outputPath" -ForegroundColor Green
    }
    
    Write-Host "✅ Publish complete" -ForegroundColor Green
    Write-Host ""
}

# Pack
if ($Pack) {
    Write-Host "📦 Creating packages..." -ForegroundColor Yellow
    
    $packagesDir = Join-Path $PublishDir "packages"
    New-Item -ItemType Directory -Force -Path $packagesDir | Out-Null
    
    foreach ($rid in (Get-ChildItem $PublishDir -Directory | Where-Object { $_.Name -ne 'packages' })) {
        $ridName = $rid.Name
        $zipFile = Join-Path $packagesDir "FileChronicle-$ridName.zip"
        
        Write-Host "  Creating $zipFile..." -ForegroundColor Cyan
        
        Compress-Archive -Path "$($rid.FullName)\*" -DestinationPath $zipFile -Force
        
        Write-Host "  ✅ Created $zipFile" -ForegroundColor Green
    }
    
    Write-Host "✅ Pack complete" -ForegroundColor Green
    Write-Host ""
}

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  Build Complete!                                             ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

if ($Publish) {
    Write-Host "Published artifacts are in: $PublishDir" -ForegroundColor Cyan
}

Write-Host "You can run the application with:" -ForegroundColor Yellow
Write-Host "  dotnet run -- snapshot C:\Test output.json" -ForegroundColor White
Write-Host ""
