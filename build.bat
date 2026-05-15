@echo off
REM === OpenRPA Build Script ===
REM Prerequisite: add MSBuild to PATH, e.g. D:\vs\MSBuild\Current\Bin

set CONFIG=Debug
set LOGFILE=build.log

echo [1/3] Restoring NuGet...
dotnet restore OpenRPA.sln --runtime win-x64 >nul 2>&1 || (echo FAILED & exit /b 1)

echo [2/3] Building OpenRPA.Interfaces...
msbuild OpenRPA.Interfaces\OpenRPA.Interfaces.csproj /p:Configuration=%CONFIG% /v:minimal >nul 2>&1 || (echo FAILED & exit /b 1)

echo [3/3] Building OpenRPA...
msbuild OpenRPA\OpenRPA.csproj /p:Configuration=%CONFIG% /v:minimal /m > %LOGFILE% 2>&1
if %ERRORLEVEL% neq 0 (
    echo FAILED - see %LOGFILE%
    findstr /i "error" %LOGFILE%
    exit /b 1
)

echo OK - debug\net48\OpenRPA.exe
