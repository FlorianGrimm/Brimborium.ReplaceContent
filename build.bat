cd "%~dp0"

pwsh -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\build.ps1' %*"