@echo off
pushd %~dp0

if exist Build goto Build
mkdir Build

:Build
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" Build.proj /m /nr:false /v:M /fl /p:SignAssembly=true;AssemblyOriginatorKeyFile=C:\Dropbox\Documents\Cain\Programming\SmtpServer\SmtpServer.pfx
if errorlevel 1 goto BuildFail
goto BuildSuccess

:BuildFail
echo.
echo *** BUILD FAILED ***
goto End

:BuildSuccess
echo.
echo **** BUILD SUCCESSFUL ***
goto end

:End
popd

pause