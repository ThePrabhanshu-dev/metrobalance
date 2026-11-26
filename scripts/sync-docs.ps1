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

Write-Host "Synced deck -> docs\index.html and docs\dmrc-proposal.html" -ForegroundColor Green
Get-ChildItem $docsDir | Select-Object Name, Length | Format-Table -AutoSize
