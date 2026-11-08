@echo off
title AutoCare Showroom Web Server
echo ========================================================
echo   DANG KHOI CHAY AUTOCARE SHOWROOM WEBSITE...
echo ========================================================
echo.

cd /d "D:\Showroom-car\CarShowroom"

:: Mo trinh duyet sau 2 giay
start "" cmd /c "timeout /t 2 /nobreak >nul && start http://localhost:5201"

:: Chay website
dotnet run --launch-profile http --no-build

:: Neu --no-build loi (chua build), thu chay build binh thuong
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Dang bien dich va khoi dong lai...
    dotnet run --launch-profile http
)

pause

