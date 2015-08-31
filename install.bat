@if "%DEBUG%" == "" @echo off
@rem ##################################################################
@rem Kaizo self-install script
@rem ##################################################################

@rem Uncomment below commands if you are experiencing problems
@rem  or if you want to rebuild also 'kaizow'

@rem NuGet restore
@rem MSBuild

setlocal
set KAIZO_DIR=%~dp0
cd bin && kaizo.exe self.compile -h %KAIZO_DIR% -d %KAIZO_DIR%/bootstrap
cd .. && xcopy bootstrap/bin bin /t
cd bootstrap && kaizo update %KAIZO_DIR%
endlocal
