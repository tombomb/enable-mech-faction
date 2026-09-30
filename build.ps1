<#
.SYNOPSIS
  Builds Mech Faction Enabler and assembles a clean mod folder in dist/.

.EXAMPLE
  ./build.ps1              # build + package to dist/MechFactionEnabler
  ./build.ps1 -Install     # ...and copy it into the game's Mods folder
  ./build.ps1 -Install -ModsDir "E:\SteamLibrary\steamapps\common\RimWorld\Mods"
#>
param(
    [switch]$Install,
    [string]$ModsDir = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$modName = "MechFactionEnabler"
$dist = Join-Path $root "dist\$modName"

dotnet build (Join-Path $root "Source\MechFactionEnabler\MechFactionEnabler.csproj") -c $Configuration -nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Only what the game needs. Source/, .git, dist/ etc. must never end up in the Mods folder
# (the Workshop uploader publishes the whole folder).
if (Test-Path $dist) { Remove-Item -Recurse -Force $dist }
New-Item -ItemType Directory -Force $dist | Out-Null
foreach ($item in "About", "1.6", "Languages", "LICENSE") {
    Copy-Item -Recurse (Join-Path $root $item) $dist
}
Write-Host "Packaged -> $dist"

if ($Install) {
    if (-not (Test-Path $ModsDir)) { throw "Mods folder not found: $ModsDir (pass -ModsDir)" }
    $target = Join-Path $ModsDir $modName
    if (Test-Path $target) {
        # Keep PublishedFileId.txt (written by the game on first Workshop upload) across reinstalls.
        $pubId = Join-Path $target "About\PublishedFileId.txt"
        $savedPubId = if (Test-Path $pubId) { Get-Content $pubId -Raw } else { $null }
        Remove-Item -Recurse -Force $target
    }
    Copy-Item -Recurse $dist $target
    if ($savedPubId) { Set-Content -NoNewline -Path (Join-Path $target "About\PublishedFileId.txt") -Value $savedPubId }
    Write-Host "Installed -> $target"
}
