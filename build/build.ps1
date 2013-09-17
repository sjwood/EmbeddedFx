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


$Global:ScriptFileInfo = New-Object System.IO.FileInfo -ArgumentList $MyInvocation.MyCommand.Definition
$Global:DirectorySeparator = [System.IO.Path]::DirectorySeparatorChar


Properties {
    $Configuration = "Debug"
    $Platform = "Any CPU"
    $Verbose = $False
}


$Global:AllowedConfigurations = "Debug", "Release"
# Note 1:
# There is an inconsistency between Solution and Project Platform values, but as we only build solutions we use the Solution definition
# see http://connect.microsoft.com/VisualStudio/feedback/details/503935/msbuild-inconsistent-platform-for-any-cpu-between-solution-and-project
#
# Note 2:
# platform specific builds (i.e. "x86", "x64", "Itanium") are not supported at present
#
$Global:AllowedPlatforms = "Any CPU"


Framework "4.0"


Task default -Depends Test


Task Help -Description "Displays information on the Tasks in this build file." {
    # Adapted code from Write-Documentation function in psake.psm1 (https://github.com/JamesKovacs/psake/blob/master/psake.psm1)

    Write-Output ("`r`n  Psake script '" + $Global:ScriptFileInfo.FullName + "' has the following Tasks defined:`r`n")

    $CurrentContext = $Psake.Context.Peek()

    if ($CurrentContext.Tasks.Default)
    {
        $DefaultTaskDependencies = $CurrentContext.Tasks.Default.DependsOn
    }
    else
    {
        $DefaultTaskDependencies = @()
    }

    foreach ($TaskKey in $CurrentContext.Tasks.Keys | Sort)
    {
        if ($TaskKey -eq "default")
        {
            continue
        }

        $Task = $CurrentContext.Tasks[$TaskKey]

        $TaskNameSuffix = ""
        if ($DefaultTaskDependencies -Contains $Task.Name)
        {
            $TaskNameSuffix = " (default)"
        }

        Write-Output ("  - " + $Task.Name + $TaskNameSuffix)
        Write-Output ("      " + $Task.Description)
        $DependenciesMessage = "No dependencies"
        if ($Task.DependsOn.Length -gt 0)
        {
            $DependenciesMessage = "Depends on: " + [System.String]::Join(", ", $Task.DependsOn)
        }
        Write-Output "      <$DependenciesMessage>`r`n"
    }
}


Task ValidateScriptProperties -Description "Validates build script properties." {
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

    $VerboseIsValid = $False
    if ($Verbose -eq $Null)
    {
        Write-Warning ("  $Verbose is Null. Verbose setting must be a boolean.")
    }
    else
    {
        if ($Verbose.GetType() -ne [System.Type]::GetType("System.Boolean"))
        {
            Write-Warning ("  Verbose property is a boolean. Provided value $Verbose of type " + $Verbose.GetType() + " will be interpreted.")
            $Verbose = [System.Convert]::ToBoolean($Verbose)
        }
        Write-Output ("  `$Verbose is set to '$Verbose'")
        $VerboseIsValid = $True
    }

    if ($ConfigurationIsValid -eq $False -Or $PlatformIsValid -eq $False -Or $VerboseIsValid -eq $False)
    {
        Throw "Either the Configuration, Platform or Verbose properties are invalid."
    }
}


Task GenerateBuildProperties -Description "Generates build properties calculated from the environment (e.g. tool versions) for use in the build process." {
    $RootDirectory = $Global:ScriptFileInfo.Directory.Parent.FullName

    $Global:BuildProperties = @()

    $Global:BuildProperties += ,("ObjDirectory", ("{0}{1}obj{1}" -f $RootDirectory, $Global:DirectorySeparator))
    $Global:BuildProperties += ,("BinDirectory", ("{0}{1}bin{1}" -f $RootDirectory, $Global:DirectorySeparator))
    $Global:BuildProperties += ,("DocDirectory", ("{0}{1}doc{1}" -f $RootDirectory, $Global:DirectorySeparator))

    $ToolsDirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList ("{0}{1}tools" -f $RootDirectory, $Global:DirectorySeparator)
    foreach ($ToolDirectoryInfo in $ToolsDirectoryInfo.GetDirectories("*", [System.IO.SearchOption]::TopDirectoryOnly))
    {
        $CurrentVersionDirectoryInfo = $Null
        foreach ($VersionDirectoryInfo in $ToolDirectoryInfo.GetDirectories("*", [System.IO.SearchOption]::TopDirectoryOnly))
        {
            if ($CurrentVersionDirectoryInfo -eq $Null -or $VersionDirectoryInfo.Name -gt $CurrentVersionDirectoryInfo.Name)
            {
                $CurrentVersionDirectoryInfo = $VersionDirectoryInfo
            }
        }

        if ($CurrentVersionDirectoryInfo -ne $Null)
        {
            $Global:BuildProperties += ,(("{0}Directory" -f $ToolDirectoryInfo.Name), ("{0}{1}" -f $CurrentVersionDirectoryInfo.FullName, $Global:DirectorySeparator))
        }
    }

    foreach ($BuildProperty in $Global:BuildProperties)
    {
        Write-Output ("  Property '" + $BuildProperty[0] + "' generated with value '" + $BuildProperty[1] + "'")
    }
}


Task CleanMSBuildPropertyFile -Description "Deletes MSBuild property file created from generated properties." {
    $BuildDirectory = $Global:ScriptFileInfo.Directory.FullName

    $PropertiesDirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList ("{0}{1}Properties" -f $BuildDirectory, $Global:DirectorySeparator)
    if ($PropertiesDirectoryInfo.Exists -eq $False)
    {
        return
    }

    foreach ($MSBuildPropertyFileInfo in $PropertiesDirectoryInfo.GetFiles("*.props", [System.IO.SearchOption]::TopDirectoryOnly))
    {
        Write-Output ("  Deleting '" + $MSBuildPropertyFileInfo.FullName + "'")
        $MSBuildPropertyFileInfo.Attributes = [System.IO.FileAttributes]::Normal
        $MSBuildPropertyFileInfo.Delete()
    }

    $PropertiesDirectoryInfo.Delete()
}


Task CreateMSBuildPropertyFileFromBuildProperties -Depends GenerateBuildProperties, CleanMSBuildPropertyFile -Description "Writes generated build properties to an MSBuild property file." {
    $BuildDirectory = $Global:ScriptFileInfo.Directory.FullName

    $PropertiesDirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList ("{0}{1}Properties" -f $BuildDirectory, $Global:DirectorySeparator)
    $PropertiesDirectoryInfo.Create()

    $MSBuildPropertyFileInfo = New-Object System.IO.FileInfo -ArgumentList ("{0}{1}MSBuild.props" -f $PropertiesDirectoryInfo.FullName, $Global:DirectorySeparator)
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


Task CreatePowershellPropertiesFromBuildProperties -Depends GenerateBuildProperties -Description "Creates in-memory Powershell variables from generated build properties." {
    foreach ($BuildProperty in $Global:BuildProperties)
    {
        New-Variable -Name $BuildProperty[0] -Value $BuildProperty[1] -Scope Script -Option Constant
        Write-Output ("  `$" + $BuildProperty[0] + " set to '" + $BuildProperty[1] + "'")
    }
}


Task Clean -Depends CleanMSBuildPropertyFile, CreatePowershellPropertiesFromBuildProperties -Description "Cleans all build products." {
    $Directories = @($ObjDirectory, $BinDirectory, $DocDirectory)
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


Task Build -Depends ValidateScriptProperties, Clean, CreateMSBuildPropertyFileFromBuildProperties -Description "Builds all source code." {
    $MSBuildVerbosity = "minimal"
    if ($Verbose -eq $True)
    {
        $MSBuildVerbosity = "normal"
    }

    $RootDirectory = $Global:ScriptFileInfo.Directory.Parent.FullName

    $SolutionDirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList ("{0}{1}sln" -f $RootDirectory, $Global:DirectorySeparator)
    foreach ($SolutionFileInfo in $SolutionDirectoryInfo.GetFiles("*.sln", [System.IO.SearchOption]::TopDirectoryOnly))
    {
        Write-Output ("  Building solution '" + $SolutionFileInfo.FullName + "'")
        MsBuild $SolutionFileInfo.FullName /nologo /verbosity:$MSBuildVerbosity /maxcpucount /property:Configuration=$Configuration /property:Platform=$Platform
        if ($LastExitCode -ne 0)
        {
            Exit 1
        }
    }
}


Task Test -Depends Build -Description "Runs all tests." {
    $XunitConsoleExe = "{0}xunit.console.exe" -f $XunitDirectory
    $TestDirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList $BinDirectory

    $HasFailingTests = $False

    foreach ($TestFileInfo in $TestDirectoryInfo.GetFiles("*.Facts.dll", [System.IO.SearchOption]::AllDirectories))
    {
        $TestOutputPath = $TestFileInfo.FullName.Replace($BinDirectory, $DocDirectory)
        $TestOutputPath = [System.IO.Path]::ChangeExtension($TestOutputPath, ".xunit.xml")
        $TestOutputFileInfo = New-Object System.IO.FileInfo -ArgumentList $TestOutputPath
        if ($TestOutputFileInfo.Directory.Exists -eq $False)
        {
            New-Item -Path $TestOutputFileInfo.Directory -Type Directory | Out-Null
        }

        Write-Output ("  Running tests in assembly '" + $TestFileInfo.FullName + "'")
        if ($Verbose -eq $True)
        {
            & $XunitConsoleExe $TestFileInfo.FullName /xml $TestOutputFileInfo.FullName
        }
        else
        {
            $StdOut = & $XunitConsoleExe $TestFileInfo.FullName /xml $TestOutputFileInfo.FullName
            Write-Output ("    " + $StdOut[$StdOut.Length - 1])
        }
        if ($LastExitCode -ne 0)
        {
            $HasFailingTests = $True
        }
    }

    $XunitTestReportDirectoryInfo = New-Object System.IO.DirectoryInfo -ArgumentList $DocDirectory

    $HasSkippedTests = $False
    foreach ($XunitTestReportFileInfo in $XunitTestReportDirectoryInfo.GetFiles("*.xunit.xml", [System.IO.SearchOption]::AllDirectories))
    {
        $SkippedTests = Select-Xml -XPath "/assembly/class/test[@result='Skip']" -Path $XunitTestReportFileInfo.FullName
        foreach ($SkippedTest in $SkippedTests)
        {
            $HasSkippedTests = $True
        }
    }

    if ($HasFailingTests -eq $True)
    {
        Write-Host -ForegroundColor Red "  Failed tests:"

        foreach ($XunitTestReportFileInfo in $XunitTestReportDirectoryInfo.GetFiles("*.xunit.xml", [System.IO.SearchOption]::AllDirectories))
        {
            $FailingTests = Select-Xml -XPath "/assembly/class/test[@result='Fail']" -Path $XunitTestReportFileInfo.FullName
            foreach ($FailingTest in $FailingTests)
            {
                Write-Host -ForegroundColor Red ("    " + $FailingTest.Node.GetAttribute("type") + "." + $FailingTest.Node.GetAttribute("method") + " in assembly " + $FailingTest.Node.ParentNode.ParentNode.GetAttribute("name"))
            }
        }
    }

    if ($HasSkippedTests -eq $True)
    {
        Write-Host -ForegroundColor Yellow "  Skipped tests:"

        foreach ($XunitTestReportFileInfo in $XunitTestReportDirectoryInfo.GetFiles("*.xunit.xml", [System.IO.SearchOption]::AllDirectories))
        {
            $SkippedTests = Select-Xml -XPath "/assembly/class/test[@result='Skip']" -Path $XunitTestReportFileInfo.FullName
            foreach ($SkippedTest in $SkippedTests)
            {
                Write-Host -ForegroundColor Yellow ("    " + $SkippedTest.Node.GetAttribute("type") + "." + $SkippedTest.Node.GetAttribute("method") + " in assembly " + $SkippedTest.Node.ParentNode.ParentNode.GetAttribute("name"))
            }
        }
    }

    if ($HasFailingTests -eq $True)
    {
        Exit 1
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