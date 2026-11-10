@echo off
title AutoCare Showroom Web Server (Hot Reload)
echo ========================================================
echo   DANG KHOI CHAY AUTOCARE SHOWROOM WEBSITE (HOT RELOAD)...
echo   He thong se TU DONG CAP NHAT khi ban sua code!
echo ========================================================
echo.

cd /d "D:\Showroom-car\CarShowroom"

:: Tat tien trinh cu neu con dang chay ngam de tranh bi khoa file (Locked)
taskkill /F /IM CarShowroom.exe >nul 2>&1

:: Mo trinh duyet sau 3 giay
start "" cmd /c "timeout /t 3 /nobreak >nul && start http://localhost:5201"

:: Chay website voi che do dotnet watch (Hot Reload & Auto-restart)
dotnet watch run --launch-profile http

pause

