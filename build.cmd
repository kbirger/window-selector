@ECHO OFF
REM SET EnableNuGetPackageRestore=true
rem IF "%1"=="install" GOTO devinstall
IF "%1"=="" SET TARGET="Build"
IF not "%1"=="" SET TARGET=%1

REM SET msbuild=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
SET msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
 %msbuild% .\build\build.proj /flp:LogFile=build.log /t:%TARGET%
GOTO done



:done
IF NOT %ERRORLEVEL% == 0 EXIT /B 1