@echo off
pushd %~dp0

REM set ShouldPublish=""
REM set NugetKey=""
REM if "%1"=="publish" set ShouldPublish="nuget"
REM if not "%2"=="" set NugetKey=%2

REM if %ShouldPublish%=="nuget" echo Publish to NuGet enabled

if exist Build goto Build
mkdir Build

:Build
REM %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild Build.proj /m /nr:false /v:M /fl /p:ShouldPublish=%ShouldPublish%;NugetKey=%NugetKey%
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