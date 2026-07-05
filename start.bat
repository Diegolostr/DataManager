@echo off
cd /d "C:\Users\dlopezst\source\repos\UnityDataImporter"

:wait
docker info >nul 2>&1
if errorlevel 1 (
    timeout /t 5 /nobreak >nul
    goto wait
)

docker compose up -d
