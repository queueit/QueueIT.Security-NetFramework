@ECHO OFF

set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319   
IF %ERRORLEVEL% NEQ 0 GOTO Error
 
call "%msBuildDir%\msbuild" "..\QueueIT.Security.sln" /p:Configuration=Release /l:FileLogger,Microsoft.Build.Engine;logfile=Build_Release.log
IF %ERRORLEVEL% NEQ 0 GOTO Error

MSTest.exe /testcontainer:"..\QueueIT.Security.Tests\bin\Release\QueueIT.Security.Tests.dll"
IF %ERRORLEVEL% NEQ 0 GOTO Error

xcopy ..\QueueIT.Security\Bin\Release\*.* /Y
IF %ERRORLEVEL% NEQ 0 GOTO Error

GOTO End

:Error
echo ###########################################
echo # ERROR OCCURED WHILE COMPILING / TESTING #
echo ###########################################

:End