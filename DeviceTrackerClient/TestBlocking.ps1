# Save as TestBlocking.ps1 and run as Administrator

Write-Host "=== TESTING APPLICATION BLOCKING ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Notepad (should work)
Write-Host "1. Testing NOTEPAD..." -ForegroundColor Yellow
$notepad = Start-Process notepad -PassThru
Start-Sleep -Seconds 2
if (Get-Process notepad -ErrorAction SilentlyContinue) {
    Write-Host "    NOTEPAD is STILL RUNNING!" -ForegroundColor Red
    Write-Host "   Process name: notepad.exe" -ForegroundColor Gray
    Stop-Process -Name notepad -Force
} else {
    Write-Host "    NOTEPAD was blocked!" -ForegroundColor Green
}

# Test 2: Task Manager
Write-Host "`n2. Testing TASK MANAGER..." -ForegroundColor Yellow
$taskmgr = Start-Process taskmgr -PassThru
Start-Sleep -Seconds 3
if (Get-Process Taskmgr -ErrorAction SilentlyContinue) {
    Write-Host "    TASK MANAGER is STILL RUNNING!" -ForegroundColor Red
    Write-Host "   Process name: Taskmgr.exe" -ForegroundColor Gray
    Write-Host "   File path: C:\Windows\System32\Taskmgr.exe" -ForegroundColor Gray
    Stop-Process -Name Taskmgr -Force
} else {
    Write-Host "    TASK MANAGER was blocked!" -ForegroundColor Green
}

# Test 3: Find Epic Games Launcher process
Write-Host "`n3. Finding EPIC GAMES LAUNCHER..." -ForegroundColor Yellow
$epicProcesses = Get-Process | Where-Object { 
    $_.ProcessName -like "*epic*" -or 
    $_.MainWindowTitle -like "*epic*" -or
    $_.ProcessName -like "*egs*"
}

if ($epicProcesses) {
    Write-Host "   Found Epic processes:" -ForegroundColor Gray
    foreach ($proc in $epicProcesses) {
        Write-Host "   • $($proc.ProcessName).exe (PID: $($proc.Id))" -ForegroundColor Gray
        try {
            $path = $proc.Path
            Write-Host "     Path: $path" -ForegroundColor DarkGray
        } catch { }
    }
} else {
    Write-Host "   No Epic processes running." -ForegroundColor Gray
    # Try to find the executable
    $epicExe = Get-ChildItem "C:\Program Files\Epic Games" -Filter *.exe -Recurse -ErrorAction SilentlyContinue | Select -First 5
    if ($epicExe) {
        Write-Host "   Found Epic executables:" -ForegroundColor Gray
        foreach ($exe in $epicExe) {
            Write-Host "   • $($exe.Name) at $($exe.DirectoryName)" -ForegroundColor Gray
        }
    }
}

# Test 4: Check block rules file
Write-Host "`n4. CHECKING BLOCK RULES..." -ForegroundColor Yellow
$rulesPath = "C:\ProgramData\DeviceTracker\block_rules.json"
if (Test-Path $rulesPath) {
    $rules = Get-Content $rulesPath | ConvertFrom-Json
    Write-Host "   Block rules found:" -ForegroundColor Gray
    foreach ($rule in $rules) {
        Write-Host "   • $($rule.Name): $($rule.Pattern) ($($rule.MatchType))" -ForegroundColor Gray
    }
} else {
    Write-Host "   No block rules file found!" -ForegroundColor Red
}

# Test 5: Check logs
Write-Host "`n5. CHECKING LOGS..." -ForegroundColor Yellow
$logDir = "C:\ProgramData\DeviceTracker\logs"
if (Test-Path $logDir) {
    $logFiles = Get-ChildItem $logDir -Filter *.json -ErrorAction SilentlyContinue
    if ($logFiles) {
        $latestLog = $logFiles | Sort LastWriteTime -Descending | Select -First 1
        Write-Host "   Latest log: $($latestLog.Name)" -ForegroundColor Gray
        $logContent = Get-Content $latestLog.FullName -Raw | ConvertFrom-Json
        $blockEvents = $logContent | Where-Object { $_.EventType -eq "AppBlocked" }
        if ($blockEvents) {
            Write-Host "   Blocking events found:" -ForegroundColor Gray
            foreach ($event in $blockEvents | Select -Last 3) {
                Write-Host "   • Blocked: $($event.AdditionalData.ProcessName) at $($event.Timestamp)" -ForegroundColor Gray
            }
        } else {
            Write-Host "   No blocking events in log." -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   No logs directory found." -ForegroundColor Yellow
}

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Cyan
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")