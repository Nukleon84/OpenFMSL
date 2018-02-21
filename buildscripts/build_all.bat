@echo off

rem Environment variables are local only and thus forgotten when script exits
setlocal

title msbuild
cd %~dp0


set DOTNET_FRAMEWORK=4.0.30319
set MSBUILD="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"
set NUGET="G:\Tools\Nuget\nuget.exe"
set ROOT=..\
if not exist %MSBUILD% goto :requirements_missing
if "%1" == "" goto :nothing_to_do

if "%1" == "all" (
  set WHAT=*.sln
) else (
  set WHAT=*%1.sln
)

if "%2" == "" (
  SET config=Debug
) else (
  SET config=%2
)

:build
set LOG=build.log
if exist %LOG% del %LOG%
echo Building: Contracts
call %MSBUILD%  /t:Build /p:Configuration=%config% ..\source.contracts\OpenFMSL.contracts\OpenFMSL.contracts.sln >> build.log 2>&1

for /r %ROOT% %%i in (%WHAT%) do (
  echo Building: %%~ni [%%i]
  call %NUGET% restore %%i
  call %MSBUILD%  /t:Build /p:Configuration=%config% %%i /nologo /verbosity:normal >> build.log 2>&1
  rem Can you read the return code of call?
)
goto end

:requirements_missing
echo ###############################################################################
echo  Ooops! You don't seem to have MSBuild or you have the wrong version.
echo  Was looking for:
echo   %MSBUILD%
echo ###############################################################################
goto end

:nothing_to_do
echo Please specify a target or 'all'.
:goto end

:end