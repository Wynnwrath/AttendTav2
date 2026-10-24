# ===================================================
#   AttendTa - Student Organization System Launcher
# ===================================================

$ErrorActionPreference = "SilentlyContinue"

Write-Host "===================================================" -ForegroundColor Cyan
Write-Host "  AttendTa - Student Organization System Launcher" -ForegroundColor Cyan
Write-Host "===================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Enable .NET major version roll-forward
$env:DOTNET_ROLL_FORWARD = "LatestMajor"

# 2. Locate the best dotnet executable
$dotnetCmd = "dotnet"
$installedSdks = & dotnet --list-sdks 2>$null
$hasNet9Or10 = $installedSdks -match "10\.|9\."

if (-not $hasNet9Or10 -and (Test-Path "C:\Program Files\dotnet\dotnet.exe")) {
    $dotnetCmd = "C:\Program Files\dotnet\dotnet.exe"
}

Write-Host "[*] Using .NET executable: $dotnetCmd" -ForegroundColor Yellow

# 3. Check and initialize LocalDB if needed
if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
    if (Test-Path "setup-db.sql") {
        Write-Host "[*] Ensuring database and tables are created..." -ForegroundColor Yellow
        & sqlcmd -S "(localdb)\MSSQLLocalDB" -i "setup-db.sql" | Out-Null
        Write-Host "[+] Database check completed." -ForegroundColor Green
    }
} else {
    Write-Host "[!] Note: sqlcmd not found in PATH; ensure AsiBasecodeDB is initialized." -ForegroundColor Gray
}

Write-Host ""
Write-Host "[*] Starting AttendTa on http://localhost:5000 ..." -ForegroundColor Cyan
Write-Host "[*] Press Ctrl+C anytime to stop." -ForegroundColor Gray
Write-Host ""

& $dotnetCmd run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"
