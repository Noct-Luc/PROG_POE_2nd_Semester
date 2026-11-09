#!/bin/bash
set -e

echo "Starting RaceDay MVC Application..."
echo "Environment: ${ASPNETCORE_ENVIRONMENT}"
echo "API Base URL: ${ApiSettings__BaseUrl}"

exec dotnet RaceDay.MVC.dll