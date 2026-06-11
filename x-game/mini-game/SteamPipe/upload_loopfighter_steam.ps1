$ErrorActionPreference = "Stop"

$appId = "3804330"
$defaultDepotId = "3804331"
$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$repoRoot = Resolve-Path (Join-Path $projectRoot "..")
$contentRoot = Get-ChildItem -LiteralPath (Join-Path $projectRoot "Builds") -Directory -Filter "SteamReview_LoopFighter_Windows_x64_*" |
    Where-Object {
        (Test-Path (Join-Path $_.FullName "LoopFighter.exe")) -and
        (Test-Path (Join-Path $_.FullName "data_MiniGame_windows_x86_64"))
    } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName
$steamCmd = Join-Path $repoRoot "Tooling\SteamCMD\steamcmd.exe"

if ([string]::IsNullOrWhiteSpace($contentRoot)) {
    throw "No Steam review build folder was found under: $(Join-Path $projectRoot "Builds")"
}

if (!(Test-Path $steamCmd)) {
    throw "steamcmd.exe was not found at: $steamCmd"
}

if (!(Test-Path (Join-Path $contentRoot "LoopFighter.exe"))) {
    throw "LoopFighter.exe was not found in content root: $contentRoot"
}

if (!(Test-Path (Join-Path $contentRoot "data_MiniGame_windows_x86_64"))) {
    throw "Godot .NET runtime data folder was not found in content root: $contentRoot"
}

$depotIdInput = Read-Host "Windows Depot ID (press Enter to use guessed $defaultDepotId)"
if ([string]::IsNullOrWhiteSpace($depotIdInput)) {
    $depotId = $defaultDepotId
} else {
    $depotId = $depotIdInput.Trim()
}

$setLiveBranch = Read-Host "Set build live on branch after upload? Leave blank for no, or enter branch name like default"
$steamUser = Read-Host "Steam uploader account name"
if ([string]::IsNullOrWhiteSpace($steamUser)) {
    throw "Steam account name is required."
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$buildOutput = Join-Path $projectRoot "Builds\SteamPipeOutput"
$scriptOutput = Join-Path $projectRoot "SteamPipe\scripts\generated"
New-Item -ItemType Directory -Force -Path $buildOutput, $scriptOutput | Out-Null

$depotVdf = Join-Path $scriptOutput "depot_build_$depotId.vdf"
$appVdf = Join-Path $scriptOutput "app_build_$appId.vdf"

@"
"DepotBuildConfig"
{
    "DepotID" "$depotId"
    "ContentRoot" "$contentRoot"
    "FileMapping"
    {
        "LocalPath" "*"
        "DepotPath" "."
        "recursive" "1"
    }
    "FileExclusion" "*.pdb"
}
"@ | Set-Content -LiteralPath $depotVdf -Encoding ASCII

$setLiveBlock = ""
if (![string]::IsNullOrWhiteSpace($setLiveBranch)) {
    $setLiveBlock = "    `"SetLive`" `"$($setLiveBranch.Trim())`"`r`n"
}

@"
"AppBuild"
{
    "AppID" "$appId"
    "Desc" "Loop Fighter Windows review build $timestamp"
    "BuildOutput" "$buildOutput"
    "ContentRoot" "$contentRoot"
$setLiveBlock    "Depots"
    {
        "$depotId" "$depotVdf"
    }
}
"@ | Set-Content -LiteralPath $appVdf -Encoding ASCII

Write-Host ""
Write-Host "App ID:      $appId"
Write-Host "Depot ID:    $depotId"
Write-Host "ContentRoot: $contentRoot"
Write-Host "App VDF:     $appVdf"
Write-Host ""
Write-Host "SteamCMD will ask for password and Steam Guard if needed."
Write-Host ""

& $steamCmd +login $steamUser +run_app_build $appVdf +quit

if ($LASTEXITCODE -ne 0) {
    throw "SteamCMD upload failed with exit code $LASTEXITCODE"
}

Write-Host ""
Write-Host "SteamCMD finished. Check Steamworks > SteamPipe > Builds to confirm the uploaded build."
