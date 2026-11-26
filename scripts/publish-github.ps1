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
    $repoExists = $false
    $prevErrPref = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & gh api "repos/$user/$RepoName" --silent 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) { $repoExists = $true }
    $ErrorActionPreference = $prevErrPref

    if (-not $repoExists) {
        Write-Host "Creating repository '$RepoName' ($Visibility)..." -ForegroundColor Cyan
        & gh repo create $RepoName --public --description $Description --disable-issues --disable-wiki | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Failed to create repository '$RepoName'." }
    } else {
        Write-Host "Repository '$RepoName' already exists - reusing it." -ForegroundColor DarkGray
    }

    # 1b. make sure 'origin' points at the GitHub repository
    $prevRemoteErr = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $currentOrigin = (& git remote get-url origin 2>&1 | Out-String).Trim()
    $ErrorActionPreference = $prevRemoteErr
    if (-not $currentOrigin -or $currentOrigin -notmatch '^https://github\.com/') {
        Write-Host "Adding remote 'origin' -> $remoteUrl" -ForegroundColor Cyan
        git remote add origin $remoteUrl
    }
    elseif ($currentOrigin -ne $remoteUrl) {
        Write-Host "Updating remote 'origin' -> $remoteUrl" -ForegroundColor Cyan
        git remote set-url origin $remoteUrl
    }

    # 2. push UAT first
    Write-Host "Pushing 'uat' branch..." -ForegroundColor Cyan
    git push -u origin uat

    # 3. push PRD and make it the default branch
    Write-Host "Pushing 'prd' branch..." -ForegroundColor Cyan
    git push -u origin prd
    gh repo edit "$user/$RepoName" --default-branch prd

    # 3b. push the legacy 'main' branch (mirror of prd) if it exists locally
    if ((git branch --list main)) {
        Write-Host "Pushing 'main' branch..." -ForegroundColor Cyan
        git push origin main
    }

    # 4. GitHub Pages served from docs/ via .github/workflows/pages.yml
    Write-Host "Configuring GitHub Pages (workflow build, docs/ on prd)..." -ForegroundColor Cyan
    $prevErrPref2 = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & gh api "repos/$user/$RepoName/pages" --silent 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        # site already exists -> switch it to the workflow build
        & gh api -X PUT "repos/$user/$RepoName/pages" -f build_type=workflow 2>&1 | Out-Null
    }
    else {
        & gh api -X POST "repos/$user/$RepoName/pages" -f build_type=workflow 2>&1 | Out-Null
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Pages API call returned non-zero - it may already be configured. Skipping." -ForegroundColor Yellow
    }
    else {
        Write-Host "GitHub Pages configured (workflow build)." -ForegroundColor Green
    }
    $ErrorActionPreference = $prevErrPref2

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
    Write-Host ('  Live demo : https://theprabhanshu-dev.github.io/metrobalance/demo/')
    Write-Host ("  Deck      : https://$user.github.io/$RepoName/")
    Write-Host ("  Repo      : https://github.com/$user/$RepoName")
}
finally {
    Pop-Location
}
