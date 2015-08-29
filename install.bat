@if "%DEBUG%" == "" @echo off
@rem ###########################
@rem # Kaizo self-install script
@rem ###########################

setlocal
set KAIZO_DIR=%~dp0
cd bootstrap
kaizo.exe
xcopy bin/ ./ /t
kaizow update %KAIZO_DIR%
endlocal
