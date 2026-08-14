<#
.SYNOPSIS
  Builds sptQuestLive and packages it into a zip with the user/mods/<ModName> layout intact.

.USAGE
  From PowerShell: .\package.ps1
  To skip rebuilding and just re-zip the existing bin\Release output: .\package.ps1 -SkipBuild
#>

param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$ModName   = "sptQuestLive"
$RootDir   = $PSScriptRoot
$BuildOut  = Join-Path $RootDir "bin\Release\$ModName"
$DistDir   = Join-Path $RootDir "dist"
$StageDir  = Join-Path $DistDir "user\mods\$ModName"
$ZipPath   = Join-Path $RootDir "$ModName.zip"

if (-not $SkipBuild) {
    Write-Host "==> dotnet build -c Release" -ForegroundColor Cyan
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed (exit code $LASTEXITCODE)"
    }
}

if (-not (Test-Path $BuildOut)) {
    throw "Build output not found: $BuildOut (build the project first)"
}

Write-Host "==> Preparing staging folder" -ForegroundColor Cyan
if (Test-Path $DistDir) { Remove-Item $DistDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $StageDir | Out-Null

Write-Host "==> Copying release files" -ForegroundColor Cyan
Copy-Item (Join-Path $BuildOut "$ModName.dll") -Destination $StageDir
Copy-Item (Join-Path $BuildOut "$ModName.deps.json") -Destination $StageDir

$DbSrc = Join-Path $BuildOut "db"
if (Test-Path $DbSrc) {
    Copy-Item $DbSrc -Destination $StageDir -Recurse
} else {
    Write-Warning "No db folder found (db/quests.json or db/locales may not exist yet)"
}

Write-Host "==> Creating zip: $ZipPath" -ForegroundColor Cyan
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Compress-Archive -Path (Join-Path $DistDir "user") -DestinationPath $ZipPath

Write-Host "==> Done" -ForegroundColor Green
Write-Host "Created: $ZipPath"
Get-ChildItem -Recurse $StageDir | ForEach-Object {
    Write-Host ("  " + $_.FullName.Substring($DistDir.Length + 1))
}
