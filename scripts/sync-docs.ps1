<#
.SYNOPSIS
    Syncs the showcase assets into docs/ so they can be published on GitHub Pages.

.DESCRIPTION
    Copies the standalone DMRC proposal deck from the Web project's wwwroot into the
    docs/ folder used by GitHub Pages:
      docs/index.html          -> published landing page (the deck)
      docs/dmrc-proposal.html  -> direct copy of the deck

    Run this before committing, then run scripts/publish-github.ps1 to push.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\sync-docs.ps1
#>
[CmdletBinding()]
param(
    [string]$ProjectRoot
)

$ErrorActionPreference = 'Stop'

if (-not $ProjectRoot) {
    $ProjectRoot = Split-Path -Parent $PSScriptRoot
}

$deckSource = Join-Path $ProjectRoot 'src\MetroBalance.Web\wwwroot\presentations\dmrc-proposal.html'
$docsDir    = Join-Path $ProjectRoot 'docs'

if (-not (Test-Path $deckSource)) {
    throw "Deck not found at '$deckSource'."
}

if (-not (Test-Path $docsDir)) {
    New-Item -ItemType Directory -Path $docsDir | Out-Null
}

Copy-Item -Path $deckSource -Destination (Join-Path $docsDir 'index.html') -Force
Copy-Item -Path $deckSource -Destination (Join-Path $docsDir 'dmrc-proposal.html') -Force

# Website preview document (screen-by-screen captures of the running site).
$previewSource = Join-Path $ProjectRoot 'src\MetroBalance.Web\wwwroot\previews'
$previewTarget = Join-Path $docsDir 'previews'
if (Test-Path $previewSource) {
    if (Test-Path $previewTarget) {
        Remove-Item -Path $previewTarget -Recurse -Force
    }
    Copy-Item -Path $previewSource -Destination $previewTarget -Recurse -Force
    Write-Host "Synced website preview -> docs\previews\" -ForegroundColor Green
}

# Static interactive demo (works on GitHub Pages without a .NET host) and the
# deck assets it links to.
$demoSource = Join-Path $ProjectRoot 'src\MetroBalance.Web\wwwroot\demo'
$demoTarget = Join-Path $docsDir 'demo'
if (Test-Path $demoSource) {
    if (Test-Path $demoTarget) {
        Remove-Item -Path $demoTarget -Recurse -Force
    }
    Copy-Item -Path $demoSource -Destination $demoTarget -Recurse -Force
    Write-Host "Synced static demo -> docs\demo\" -ForegroundColor Green
}

$presentationSource = Join-Path $ProjectRoot 'src\MetroBalance.Web\wwwroot\presentations'
$presentationTarget = Join-Path $docsDir 'presentations'
if (Test-Path $presentationSource) {
    if (Test-Path $presentationTarget) {
        Remove-Item -Path $presentationTarget -Recurse -Force
    }
    Copy-Item -Path $presentationSource -Destination $presentationTarget -Recurse -Force
    Write-Host "Synced deck assets -> docs\presentations\" -ForegroundColor Green
}

Write-Host "Synced deck -> docs\index.html and docs\dmrc-proposal.html" -ForegroundColor Green
Get-ChildItem $docsDir | Select-Object Name, Length | Format-Table -AutoSize
