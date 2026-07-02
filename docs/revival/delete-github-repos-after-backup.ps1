param(
    [string]$Owner = "kongliuli",
    [Parameter(Mandatory = $true)]
    [string[]]$Repo,
    [switch]$ConfirmRemoteDeletion
)

$ErrorActionPreference = "Stop"

if (-not $ConfirmRemoteDeletion) {
    throw "Refusing to delete GitHub repositories without -ConfirmRemoteDeletion."
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$backupRoot = Join-Path $root "_github-delete-backups"
$gh = "C:\Program Files\GitHub CLI\gh.exe"

if (-not (Test-Path $gh)) {
    throw "GitHub CLI not found: $gh"
}

foreach ($name in $Repo) {
    $worktree = Join-Path $root $name
    $mirror = Join-Path $backupRoot "$name.git"

    if (-not (Test-Path $worktree) -and -not (Test-Path $mirror)) {
        throw "No local retention found for $name. Expected $worktree or $mirror."
    }

    Write-Host "Deleting GitHub repo $Owner/$name; local retention verified."
    & $gh repo delete "$Owner/$name" --yes
}
