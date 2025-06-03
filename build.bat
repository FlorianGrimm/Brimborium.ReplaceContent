@echo off
cd "%~dp0"
set cp=%~dp0

pwsh -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\build.ps1' %*"
