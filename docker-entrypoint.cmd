@echo off
setlocal enabledelayedexpansion

echo Starting RaceDay MVC Application...
echo Environment: %ASPNETCORE_ENVIRONMENT%
echo API Base URL: %ApiSettings__BaseUrl%

dotnet RaceDay.MVC.dll