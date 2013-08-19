# Copyright 2012-2013 Steve Wood
# 
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# 
#     http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.


Set-StrictMode -Version Latest


Properties {
    $Configuration = "Debug"
    $Platform = "AnyCPU"
}


$Global:AllowedConfigurations = "Debug", "Release"
# Note 1:
# There is an inconsistency between Solution and Project Platform values, so use Project definition for now
# see http://connect.microsoft.com/VisualStudio/feedback/details/503935/msbuild-inconsistent-platform-for-any-cpu-between-solution-and-project
#
# Note 2:
# platform specific builds (i.e. "x86", "x64", "Itanium") are not supported at present
#
$Global:AllowedPlatforms = "AnyCPU"


Framework "4.0"


Task default -Depends Test


Task ValidateScriptProperties -Description "Validates build script properties" {
    $ConfigurationIsValid = IsValueInSet $Configuration $Global:AllowedConfigurations
    if ($ConfigurationIsValid -eq $False)
    {
        Write-Warning "  '$Configuration' is not an allowed configuration. Allowed configurations are $Global:AllowedConfigurations."
    }
    else
    {
        Write-Output ("  `$Configuration is set to '$Configuration'")
    }

    $PlatformIsValid = IsValueInSet $Platform $Global:AllowedPlatforms
    if ($PlatformIsValid -eq $False)
    {
        Write-Warning "  '$Platform' is not an allowed platform. Allowed platforms are $Global:AllowedPlatforms."
    }
    else
    {
        Write-Output ("  `$Platform is set to '$Platform'")
    }


    if ($ConfigurationIsValid -eq $False -Or $PlatformIsValid -eq $False)
    {
        Throw "Either the build Configuration or Platform is invalid."
    }
}


Task GenerateBuildProperties -Description "Generates build properties calculated from the environment (e.g. tool versions) for use in the build process" {
    $Global:BuildProperties = @()

    $Global:BuildProperties += ,("ObjDirectory", ((New-Object System.IO.DirectoryInfo -ArgumentList "..\obj").FullName + "\"))
    $Global:BuildProperties += ,("BinDirectory", ((New-Object System.IO.DirectoryInfo -ArgumentList "..\bin").FullName + "\"))

    $ToolsDirectory = New-Object System.IO.DirectoryInfo -ArgumentList "..\tools"
    foreach ($ToolDirectory in $ToolsDirectory.GetDirectories("*", [System.IO.SearchOption]::TopDirectoryOnly))
    {
        $CurrentVersionDirectory = $Null
        foreach ($VersionDirectory in $ToolDirectory.GetDirectories("*", [System.IO.SearchOption]::TopDirectoryOnly))
        {
            if ($CurrentVersionDirectory -eq $Null -or $VersionDirectory.Name -gt $CurrentVersionDirectory.Name)
            {
                $CurrentVersionDirectory = $VersionDirectory
            }
        }

        if ($CurrentVersionDirectory -ne $Null)
        {
            $Global:BuildProperties += ,(($ToolDirectory.Name + "Directory"), ($CurrentVersionDirectory.FullName + "\"))
        }
    }

    foreach ($BuildProperty in $Global:BuildProperties)
    {
        Write-Output ("  Property '" + $BuildProperty[0] + "' generated with value '" + $BuildProperty[1] + "'")
    }
}


Task CleanMSBuildPropertyFile -Description "Deletes MSBuild property file created from generated properties" {
    $PropertiesDirectory = New-Object System.IO.DirectoryInfo -ArgumentList ".\Properties"
    if ($PropertiesDirectory.Exists -eq $False)
    {
        return
    }

    foreach ($MSBuildPropertyFileInfo in $PropertiesDirectory.GetFiles("*.props", [System.IO.SearchOption]::TopDirectoryOnly))
    {
        Write-Output ("  Deleting '" + $MSBuildPropertyFileInfo.FullName + "'")
        $MSBuildPropertyFileInfo.Attributes = [System.IO.FileAttributes]::Normal
        $MSBuildPropertyFileInfo.Delete()
    }

    $PropertiesDirectory.Delete()
}


Task CreateMSBuildPropertyFileFromBuildProperties -Depends GenerateBuildProperties, CleanMSBuildPropertyFile -Description "Writes generated build properties to an MSBuild property file" {
    $PropertiesDirectory = New-Object System.IO.DirectoryInfo -ArgumentList ".\Properties"
    $PropertiesDirectory.Create()

    $MSBuildPropertyFileInfo = New-Object System.IO.FileInfo -ArgumentList ($PropertiesDirectory.FullName + "\MSBuild.props")
    Write-Output ("  Creating '" + $MSBuildPropertyFileInfo.FullName + "'")

    $XmlWriter = New-Object System.Xml.XmlTextWriter $MSBuildPropertyFileInfo.FullName, ([System.Text.Encoding]::UTF8)
    $XmlWriter.Formatting = [System.Xml.Formatting]::Indented
    $XmlWriter.Indentation = 4
    $XmlWriter.WriteStartDocument()
    $XmlWriter.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003")
    $XmlWriter.WriteStartElement("PropertyGroup")

    foreach ($BuildProperty in $Global:BuildProperties)
    {
        $XmlWriter.WriteElementString($BuildProperty[0], $BuildProperty[1])
    }

    $XmlWriter.WriteEndElement()
    $XmlWriter.WriteEndElement()
    $XmlWriter.WriteEndDocument()
    $XmlWriter.Close()
}


Task CreatePowershellPropertiesFromBuildProperties -Depends GenerateBuildProperties -Description "Creates in-memory Powershell variables from generated build properties" {
    foreach ($BuildProperty in $Global:BuildProperties)
    {
        New-Variable -Name $BuildProperty[0] -Value $BuildProperty[1] -Scope Script -Option Constant
        Write-Output ("  `$" + $BuildProperty[0] + " set to '" + $BuildProperty[1] + "'")
    }
}


Task Clean -Depends CleanMSBuildPropertyFile, CreatePowershellPropertiesFromBuildProperties -Description "Cleans all build products" {
    $Directories = @($ObjDirectory, $BinDirectory)
    foreach ($Directory in $Directories)
    {
        $DirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList $Directory
        if ($DirectoryInfo.Exists)
        {
            Write-Output ("  Removing directory '" + $DirectoryInfo.FullName + "'")
            Remove-Item $DirectoryInfo.FullName -Force -Recurse
        }
    }
}


Task Build -Depends ValidateScriptProperties, Clean, CreateMSBuildPropertyFileFromBuildProperties -Description "Builds all source code" {
    $SolutionDirectory = New-Object System.IO.DirectoryInfo -ArgumentList "..\sln"
    foreach ($SolutionFile in $SolutionDirectory.GetFiles("*.sln", [System.IO.SearchOption]::TopDirectoryOnly))
    {
        Write-Output ("  Building solution '" + $SolutionFile.FullName + "'")
        Exec {
            MsBuild $SolutionFile.FullName /nologo /verbosity:minimal /maxcpucount /p:Configuration=$Configuration /p:Platform=$Platform
        }
    }
}


Task Test -Depends Build -Description "Runs all tests" {
    $XunitConsoleExe = [System.IO.Path]::Combine($XunitDirectory, "xunit.console.exe")
    $TestDirectory = New-Object System.IO.DirectoryInfo -ArgumentList $BinDirectory
    foreach ($TestFile in $TestDirectory.GetFiles("*.Facts.dll", [System.IO.SearchOption]::AllDirectories))
    {
        Write-Output ("  Running tests in assembly '" + $TestFile.FullName + "'")
        Exec {
            $StdOut = & $XunitConsoleExe $TestFile.FullName /silent
            Write-Output ("    " + $StdOut[$StdOut.Length - 1])
        }
    }
}


function IsValueInSet([string] $Value, [string[]] $Set)
{
    foreach ($SetItem in $Set)
    {
        if ($Value -eq $SetItem)
        {
            return $True
        }
    }
    return $False
}