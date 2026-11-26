<#
.SYNOPSIS
    Publishes the MetroBalance repository to GitHub: UAT branch first, then PRD (default branch).

.DESCRIPTION
    Requires an authenticated GitHub CLI session. Run once:
        gh auth login

    The script then:
      1. creates the repository (public) if it does not exist,
      2. pushes the 'uat' branch first,
      3. pushes the 'prd' branch and makes it the repository default branch,
      4. enables GitHub Pages from the 'docs/' folder on the PRD branch,
      5. prints the shareable links to paste into the DMRC e-mail.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/publish-github.ps1
    powershell -ExecutionPolicy Bypass -File scripts/publish-github.ps1 -RepoName metrobalance -Visibility private
#>
[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [string]$RepoName     = 'metrobalance',
    [string]$Description  = 'MetroBalance - free, database-less Delhi Metro Smart Card balance & history enquiry service (.NET 10 MVC + Web API, demo data).',
    [ValidateSet('public', 'private')]
    [string]$Visibility   = 'public'
)

$ErrorActionPreference = 'Stop'

if (-not $ProjectRoot) {
    $ProjectRoot = Split-Path -Parent $PSScriptRoot
}

# --- preflight -----------------------------------------------------------------
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI ('gh') is not installed or not on PATH."
}

gh auth status | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Not authenticated with GitHub. Run: gh auth login"
}

Push-Location $ProjectRoot

try {
    $user = (gh api user --jq .login).Trim()
    $remoteUrl = "https://github.com/$user/$RepoName.git"

    # 1. create the repository if needed
    $exists = gh repo view $RepoName 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Creating repository '$RepoName' ($Visibility)..." -ForegroundColor Cyan
        gh repo create $RepoName --public --description $Description --disable-issues --disable-wiki | Out-Null
    } else {
        Write-Host "Repository '$RepoName' already exists - reusing it." -ForegroundColor DarkGray
    }

    # 2. push UAT first
    Write-Host "Pushing 'uat' branch..." -ForegroundColor Cyan
    git push -u origin uat

    # 3. push PRD and make it the default branch
    Write-Host "Pushing 'prd' branch..." -ForegroundColor Cyan
    git push -u origin prd
    gh repo edit $RepoName --default-branch prd

    # 4. GitHub Pages from /docs on the PRD branch
    Write-Host "Enabling GitHub Pages (docs/ on prd)..." -ForegroundColor Cyan
    gh api -X POST "repos/$user/$RepoName/pages" `
        -f source[branch]=prd -f source[path]=/docs 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Pages API call returned non-zero - it may already be configured. Skipping." -ForegroundColor Yellow
    }

    # 5. show the links
    Write-Host ''
    Write-Host '=============================================================' -ForegroundColor Green
    Write-Host ' PUBLISHED' -ForegroundColor Green
    Write-Host '=============================================================' -ForegroundColor Green
    Write-Host "Repository      : https://github.com/$user/$RepoName"
    Write-Host "Deck / Pages    : https://$user.github.io/$RepoName/   (may take a few minutes to publish)"
    Write-Host "Direct deck     : https://$user.github.io/$RepoName/dmrc-proposal.html"
    Write-Host "Default branch  : prd   (uat pushed first)"
    Write-Host ''
    Write-Host 'Paste these into the DMRC e-mail:' -ForegroundColor Cyan
    Write-Host ('  Live demo : {{LIVE_URL}}')
    Write-Host ("  Deck      : https://$user.github.io/$RepoName/")
    Write-Host ("  Repo      : https://github.com/$user/$RepoName")
}
finally {
    Pop-Location
}
