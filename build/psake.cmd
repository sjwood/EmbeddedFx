@ECHO OFF


REM Set environment variables
SET PSAKE_VERSION=4.1.0


REM Check for PoSh v2 (or greater)
IF NOT EXIST %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe (
	SET EXITCODE=1
	GOTO NO_POSH_V2
)
Powershell -Command "& { if ($Host.Version.Major -lt 2) { Exit 1; } Exit 0; }"
SET EXITCODE=%ERRORLEVEL%
IF %EXITCODE% GTR 0 (
	GOTO NO_POSH_V2
)


REM Run psake
PUSHD %~dp0
Powershell -Version 2.0 -Command "& { $CurrentExecutionPolicy = Get-ExecutionPolicy -Scope CurrentUser; if ($CurrentExecutionPolicy -gt [Microsoft.PowerShell.ExecutionPolicy]::RemoteSigned) { Set-ExecutionPolicy -Scope CurrentUser RemoteSigned; } ..\tools\psake\%PSAKE_VERSION%\psake.ps1 %*; if ($CurrentExecutionPolicy -gt [Microsoft.PowerShell.ExecutionPolicy]::RemoteSigned) { Set-ExecutionPolicy -Scope CurrentUser $CurrentExecutionPolicy; } if ($psake.build_success -eq $False) { Exit 1; } Exit 0; }"
SET EXITCODE=%ERRORLEVEL%
POPD
GOTO END


:NO_POSH_V2
ECHO PowerShell version 2 or greater required. Install it from http://www.microsoft.com/powershell. 1>&2
GOTO END


:END
EXIT /B %EXITCODE%