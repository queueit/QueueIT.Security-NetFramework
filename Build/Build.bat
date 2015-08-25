@ECHO OFF
set ERRORLEVEL=0

call "msbuild" "..\QueueIT.Security.sln" /p:Configuration=Release /l:FileLogger,Microsoft.Build.Engine;logfile=Build_Release.log
IF %ERRORLEVEL% NEQ 0 GOTO Error

MSTest.exe /testcontainer:"..\QueueIT.Security.Tests\bin\Release\QueueIT.Security.Tests.dll"
IF %ERRORLEVEL% NEQ 0 GOTO Error

xcopy ..\QueueIT.Security\Bin\Release\*.* /Y
IF %ERRORLEVEL% NEQ 0 GOTO Error
xcopy ..\QueueIT.Security.Mvc\Bin\Release\QueueIT.Security.Mvc.dll /Y
IF %ERRORLEVEL% NEQ 0 GOTO Error
xcopy ..\QueueIT.Security.Mvc\Bin\Release\QueueIT.Security.Mvc.xml /Y
IF %ERRORLEVEL% NEQ 0 GOTO Error

mkdir doc
xcopy ..\QueueIT.Security.Documentation\Help doc /Y /E



GOTO End

:Error
echo ###########################################
echo # ERROR OCCURED WHILE COMPILING / TESTING #
echo ###########################################

:End