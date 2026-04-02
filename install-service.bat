@echo off
echo Installing PRoCon Service...
sc create PRoConService binPath="%~dp0Service\PRoCon.Service.exe" start=auto DisplayName="PRoCon Frostbite Service"
sc description PRoConService "PRoCon Frostbite RCON Tool - Background Service"
echo.
echo Service installed. Start with: sc start PRoConService
pause
