@REM Copyright 2012-2013 Steve Wood
@REM 
@REM Licensed under the Apache License, Version 2.0 (the "License");
@REM you may not use this file except in compliance with the License.
@REM You may obtain a copy of the License at
@REM 
@REM     http://www.apache.org/licenses/LICENSE-2.0
@REM 
@REM Unless required by applicable law or agreed to in writing, software
@REM distributed under the License is distributed on an "AS IS" BASIS,
@REM WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
@REM See the License for the specific language governing permissions and
@REM limitations under the License.


@ECHO OFF
SETLOCAL


REM Set environment variables
SET PSAKE_VERSION=4.1.0


REM Check for PoSh v2 (or greater)
IF NOT EXIST %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe (
	SET EXITCODE=1
	GOTO NO_POSH_V2
)
Powershell ^
$ErrorActionPreference = """Stop"""; ^
if ($Host.Version.Major -ge 2) ^
{ ^
  Exit 0; ^
} ^
Exit 1;
SET EXITCODE=%ERRORLEVEL%
IF %EXITCODE% GTR 0 (
	GOTO NO_POSH_V2
)


REM Check for .NET Framework 4.0 (Full Framework)
Powershell ^
$ErrorActionPreference = """Stop"""; ^
if ((Test-Path -Path """HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4""") -eq $True) ^
{ ^
  if ((Test-Path -Path """HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full""") -eq $True) ^
  { ^
    Exit 0; ^
  } ^
} ^
Exit 1;
SET EXITCODE=%ERRORLEVEL%
IF %EXITCODE% GTR 0 (
	GOTO NO_NETFX_V4
)


REM Check for .NET Framework 3.5 (can't do multitarget build for .NET Framework 2.0 otherwise...)
Powershell ^
$ErrorActionPreference = """Stop"""; ^
if ((Test-Path -Path """HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5""") -eq $True) ^
{ ^
  Exit 0; ^
} ^
Exit 1;
SET EXITCODE=%ERRORLEVEL%
IF %EXITCODE% GTR 0 (
	GOTO NO_NETFX_V35
)


REM Run psake
PUSHD %~dp0
Powershell -Version 2.0 ^
$ErrorActionPreference = """Stop"""; ^
$CurrentExecutionPolicy = Get-ExecutionPolicy -Scope CurrentUser; ^
if ($CurrentExecutionPolicy -gt [Microsoft.PowerShell.ExecutionPolicy]::RemoteSigned) ^
{ ^
  Set-ExecutionPolicy -Scope CurrentUser RemoteSigned; ^
} ^
Import-Module """%~dp0Psake\%PSAKE_VERSION%\psake.psm1"""; ^
Invoke-psake -buildFile """%~dp0build.ps1""" %*; ^
$ReturnCode = 1; ^
if ($Psake.Build_Success -eq $True) ^
{ ^
  $ReturnCode = 0; ^
} ^
Remove-Module -Name psake; ^
if ($CurrentExecutionPolicy -gt [Microsoft.PowerShell.ExecutionPolicy]::RemoteSigned) ^
{ ^
  Set-ExecutionPolicy -Scope CurrentUser $CurrentExecutionPolicy; ^
} ^
Exit $ReturnCode;
SET EXITCODE=%ERRORLEVEL%
POPD
GOTO END


:NO_POSH_V2
	ECHO PowerShell version 2 or greater is required. Install it from http://www.microsoft.com/powershell. 1>&2
	GOTO END


:NO_NETFX_V4
	ECHO .NET Framework version 4 (Full Framework) or greater is required. Install it from http://www.microsoft.com/net/download/version-4. 1>&2
	GOTO END


:NO_NETFX_V35
	ECHO .NET Framework version 3.5 is required. Install it from http://www.microsoft.com/download/details.aspx?id=22. 1>&2
	GOTO END


:END
	EXIT /B %EXITCODE%
	ENDLOCAL