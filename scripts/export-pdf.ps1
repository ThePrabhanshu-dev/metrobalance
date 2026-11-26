<#
.SYNOPSIS
    Exports the showcase deck to a 13-page PDF for e-mailing to DMRC.

.DESCRIPTION
    Uses headless Chrome (or Edge) to render
    src\MetroBalance.Web\wwwroot\presentations\dmrc-proposal.html and honour the
    deck's print stylesheet (@page { size: 1200px 700px }), producing one page per slide.

    Output: docs\MetroBalance-DMRC-Proposal.pdf

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\export-pdf.ps1
    powershell -ExecutionPolicy Bypass -File scripts\export-pdf.ps1 -BrowserEdge
#>
[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [switch]$BrowserEdge
)

$ErrorActionPreference = 'Stop'

if (-not $ProjectRoot) {
    $ProjectRoot = Split-Path -Parent $PSScriptRoot
}

$deckPath = Join-Path $ProjectRoot 'src\MetroBalance.Web\wwwroot\presentations\dmrc-proposal.html'
$outPath  = Join-Path $ProjectRoot 'docs\MetroBalance-DMRC-Proposal.pdf'

if (-not (Test-Path $deckPath)) { throw "Deck not found: $deckPath" }

$candidates = if ($BrowserEdge) {
    @(
        "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
        "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
    )
} else {
    @(
        "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
        "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe",
        "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
        "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
    )
}

$browser = $candidates | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $browser) { throw 'No Chrome or Edge executable found for headless PDF export.' }

$uri = ([System.Uri](Resolve-Path $deckPath).Path).AbsoluteUri
Write-Host "Rendering $uri" -ForegroundColor Cyan
Write-Host "Using $browser" -ForegroundColor DarkGray

$prevErrPref = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
& $browser --headless=new --disable-gpu --no-pdf-header-footer `
    --print-to-pdf="$outPath" --virtual-time-budget=8000 $uri 2>&1 | Out-Null
$ErrorActionPreference = $prevErrPref

if (-not (Test-Path $outPath)) { throw "PDF was not created at $outPath" }
$size = [math]::Round((Get-Item $outPath).Length / 1KB)
Write-Host "PDF written: $outPath ($size KB)" -ForegroundColor Green
