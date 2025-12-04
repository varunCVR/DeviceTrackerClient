@echo off
echo Building DeviceTrackerConfig...
echo.

REM Build the config tool
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" DeviceTrackerConfig.csproj /p:Configuration=Debug

if %errorLevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Running ConfigTool...
echo.
echo Instructions:
echo 1. Save Settings - Test saving
echo 2. Install Service - Will ask to build main project
echo 3. View Logs - Open log folder
echo 4. Uninstall - Test password dialog
echo.

echo Press any key to start ConfigTool...
pause >nul

start "" "bin\Debug\DeviceTrackerConfig.exe"