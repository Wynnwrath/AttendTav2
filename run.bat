@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo   AttendTa - Student Organization System Launcher
echo ===================================================
echo.

:: 1. Enable .NET major version roll-forward
set DOTNET_ROLL_FORWARD=LatestMajor

:: 2. Find the best dotnet.exe path
set "DOTNET_CMD=dotnet"

:: Check if standard dotnet supports .NET 9/10
dotnet --list-sdks 2>nul | findstr /R "10\.[0-9] 9\.[0-9]" >nul
if %errorlevel% neq 0 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        set "DOTNET_CMD=C:\Program Files\dotnet\dotnet.exe"
    )
)

echo [*] Using .NET executable: !DOTNET_CMD!
echo.

:: 3. Check and initialize LocalDB if needed
echo [*] Checking database setup...
where sqlcmd >nul 2>nul
if %errorlevel% equ 0 (
    if exist "setup-db.sql" (
        sqlcmd -S "(localdb)\MSSQLLocalDB" -i "setup-db.sql" >nul 2>nul
        echo [+] Database check completed.
    )
) else (
    echo [!] sqlcmd not in PATH, skipping automatic DB init.
)
echo.

:: 4. Start the application
echo [*] Starting AttendTa on http://localhost:5000 ...
echo [*] Press Ctrl+C anytime to stop.
echo.

"!DOTNET_CMD!" run --project "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj"

pause
