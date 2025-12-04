@echo off
echo ============================================
echo Device Tracker Installation Script
echo ============================================
echo.

REM Check admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires Administrator privileges!
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo Step 1: Building projects...
echo.

REM Build both projects
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" DeviceTrackerClient.sln /p:Configuration=Release /p:Platform="Any CPU"

if %errorLevel% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo Step 2: Creating installation directory...
echo.

REM Create program files directory
set "INSTALL_DIR=C:\Program Files\Device Tracker"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

echo Step 3: Copying files...
echo.

REM Copy files from DeviceTrackerClient
xcopy "DeviceTrackerClient\bin\Release\*.*" "%INSTALL_DIR%\" /E /Y /I

REM Copy files from DeviceTrackerConfig
xcopy "DeviceTrackerConfig\bin\Release\*.*" "%INSTALL_DIR%\" /E /Y /I

echo Step 4: Installing service...
echo.

REM Install service
cd "%INSTALL_DIR%"
sc create DeviceTrackerService binPath= "%INSTALL_DIR%\DeviceTrackerClient.exe" start= auto DisplayName= "Device Tracker Service"
sc description DeviceTrackerService "Monitors application usage and system events"

echo Step 5: Starting service...
echo.
sc start DeviceTrackerService

echo Step 6: Creating shortcuts...
echo.

REM Create desktop shortcut
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\shortcut.vbs"
echo sLinkFile = "%USERPROFILE%\Desktop\Device Tracker Config.lnk" >> "%TEMP%\shortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\shortcut.vbs"
echo oLink.TargetPath = "%INSTALL_DIR%\DeviceTrackerConfig.exe" >> "%TEMP%\shortcut.vbs"
echo oLink.WorkingDirectory = "%INSTALL_DIR%" >> "%TEMP%\shortcut.vbs"
echo oLink.Description = "Device Tracker Configuration" >> "%TEMP%\shortcut.vbs"
echo oLink.Save >> "%TEMP%\shortcut.vbs"
cscript //nologo "%TEMP%\shortcut.vbs"
del "%TEMP%\shortcut.vbs"

echo ============================================
echo Installation Complete!
echo ============================================
echo.
echo Installed to: %INSTALL_DIR%
echo Service: DeviceTrackerService (Auto-start)
echo Config Tool: Desktop shortcut created
echo.
echo Run the config tool to set up server connection.
echo.
pause