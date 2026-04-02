@echo off
echo Stopping and removing PRoCon Service...
sc stop PRoConService >nul 2>&1
sc delete PRoConService
echo.
echo Service removed.
pause
