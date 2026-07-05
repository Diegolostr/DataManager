@echo off
timeout /t 30 /nobreak >nul

cd /d "C:\Users\dlopezst\source\repos\UnityDataImporter"

:wait
docker info >nul 2>&1
if errorlevel 1 (
    timeout /t 5 /nobreak >nul
    goto wait
)

docker compose up -d

$action = New-ScheduledTaskAction -Execute "C:\Users\dlopezst\source\repos\UnityDataImporter\start.bat"
$trigger = New-ScheduledTaskTrigger -AtLogon
$trigger.Delay = "PT1M"
$settings = New-ScheduledTaskSettingsSet -ExecutionTimeLimit (New-TimeSpan -Minutes 10)
Register-ScheduledTask -TaskName "UnityDataImporter" -Action $action -Trigger $trigger -Settings $settings -RunLevel Highest -Force
