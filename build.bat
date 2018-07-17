@echo off

if [%1]==[] goto usage

if "%1"=="Windows64" (
    set TARGET=
    goto :param_ok
)

if "%1"=="Windows" (
    set TARGET=_32bit
    goto :param_ok
)

if "%1"=="Linux64" (
    set TARGET=_linux
    goto :param_ok
)

goto :param1_error

:param_ok

IF "%UNITY_PATH%"=="" (
    echo 'UNITY_PATH environment variable not found'
    exit /b -1
)

set OUT_DIR=out
set BUILD_DIR=%OUT_DIR%\build

if exist %BUILD_DIR% (
    del /f /s /q %BUILD_DIR% 1>nul
    rmdir /s /q %BUILD_DIR%
)

set VERSION=
REM Save stdout in VERSION
for /f %%i in ('git tag -l --points-at HEAD') do set VERSION=%%i

IF "%VERSION%"=="" (
    set VERSION=_draft
) else (
    set VERSION=%VERSION:~1%
)

echo Building version %VERSION%%TARGET%

set OUT_FOLDER_NAME=SpaceDevOps%TARGET%
set ZIP_NAME=SpaceDevOps%VERSION%%TARGET%.zip
set OUT_PATH=%BUILD_DIR%\%OUT_FOLDER_NAME%

%UNITY_PATH% -quit -batchmode -build%1Player %OUT_PATH%\SpaceDevOps.exe

copy Dev\UserReadme.txt %OUT_PATH%\README.txt
xcopy Dev\ThirdPartyLicenses %OUT_PATH%\ThirdPartyLicenses\
echo %VERSION%%TARGET% > %OUT_PATH%\version.txt

CScript Tools\zip.vbs %BUILD_DIR%\ %OUT_DIR%\%ZIP_NAME%

echo Done
goto :eof

:usage
@echo Usage: %0 ^<Target^>
exit /b 1

:param1_error
@echo Target parameter shall be either Windows64, Windows or Linux64
exit /b 1
