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


Task ValidateProperties -Description "Validates script properties" {
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


Task GenerateProperties -Description "Generates build properties calculated from the environment (e.g. tool versions) for use in the build process" {
    $Global:GeneratedProperties = @()

    $Global:GeneratedProperties += ,("ObjDirectory", ((New-Object System.IO.DirectoryInfo -ArgumentList "..\obj").FullName + "\"))
    $Global:GeneratedProperties += ,("BinDirectory", ((New-Object System.IO.DirectoryInfo -ArgumentList "..\bin").FullName + "\"))

    $ToolsDirectory = New-Object System.IO.DirectoryInfo -ArgumentList "..\tools"
    :ToolDirectoryLoop foreach ($ToolDirectory in $ToolsDirectory.GetDirectories("*", [System.IO.SearchOption]::TopDirectoryOnly)) {
        $CurrentVersionDirectory = $Null
        :VersionDirectoryLoop foreach ($VersionDirectory in $ToolDirectory.GetDirectories("*", [System.IO.SearchOption]::TopDirectoryOnly)) {
            if ($CurrentVersionDirectory -eq $Null -or $VersionDirectory.Name -gt $CurrentVersionDirectory.Name)
            {
                $CurrentVersionDirectory = $VersionDirectory
            }
        }
        if ($CurrentVersionDirectory -ne $Null)
        {
            $Global:GeneratedProperties += ,(($ToolDirectory.Name + "Directory"), ($CurrentVersionDirectory.FullName + "\"))
        }
    }

    :GeneratedPropertyLoop foreach ($GeneratedProperty in $Global:GeneratedProperties) {
        Write-Output ("  Property '" + $GeneratedProperty[0] + "' generated with value '" + $GeneratedProperty[1] + "'")
    }
}


Task CleanMSBuildPropertyFile -Description "Deletes MSBuild property file created from generated properties" {
    $PropertiesDirectory = New-Object System.IO.DirectoryInfo -ArgumentList ".\Properties"
    if ($PropertiesDirectory.Exists -eq $False)
    {
        return
    }

    :MSBuildPropertyFileInfoLoop foreach ($MSBuildPropertyFileInfo in $PropertiesDirectory.GetFiles("*.props", [System.IO.SearchOption]::TopDirectoryOnly)) {
        Write-Output ("  Deleting '" + $MSBuildPropertyFileInfo.FullName + "'")
        $MSBuildPropertyFileInfo.Attributes = [System.IO.FileAttributes]::Normal
        $MSBuildPropertyFileInfo.Delete()
    }
}


Task CreateMSBuildPropertyFileFromGeneratedProperties -Depends GenerateProperties, CleanMSBuildPropertyFile -Description "Writes generated build properties to an MSBuild property file" {
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
    :GeneratedPropertyLoop foreach ($GeneratedProperty in $Global:GeneratedProperties) {
        $XmlWriter.WriteElementString($GeneratedProperty[0], $GeneratedProperty[1])
    }
    $XmlWriter.WriteEndElement()
    $XmlWriter.WriteEndElement()
    $XmlWriter.WriteEndDocument()
    $XmlWriter.Close()
}


Task CreatePowershellPropertiesFromGeneratedProperties -Depends GenerateProperties -Description "Creates in-memory Powershell variables from generated build properties" {
    Write-Host -Fore Red "TODO!!!"
}


Task Clean -Depends InstantiateBuildHelper -Description "Cleans all build products" {
    $Global:BuildHelper.Clean()
}


Task Build -Depends ValidateProperties, Clean, InstantiateBuildHelper, CreateMSBuildPropertyFileFromGeneratedProperties -Description "Builds all source code" {
    $SolutionFiles = $Global:BuildHelper.GetSolutionFiles()
    :SolutionFileLoop foreach ($SolutionFile in $SolutionFiles) {
        Write-Output ("  Building solution '" + $SolutionFile.FullName + "'")
        Exec { MsBuild $SolutionFile.FullName /nologo /verbosity:minimal /maxcpucount /p:Configuration=$Configuration /p:Platform=$Platform }
    }
}


Task Test -Depends Build, InstantiateBuildHelper -Description "Runs all tests" {
    $XunitConsoleExe = [System.IO.Path]::Combine($Global:BuildHelper.GetToolDirectory("Xunit").FullName, "xunit.console.exe")
    $TestFiles = $Global:BuildHelper.GetTestFiles()
    :TestFileLoop foreach ($TestFile in $TestFiles) {
        Exec { & $XunitConsoleExe $TestFile.FullName }
    }
}


Task InstantiateBuildHelper -Description "Creates a global Build.Helper object" {

    $BuildHelperType = "
    namespace Build
    {
        using System;
        using System.Collections.Generic;
        using System.IO;

        public class Helper
        {
            public Helper(string[] allowedConfigurations, string[] allowedPlatforms)
            {
                this.SetDirectories();
                this.DiscoverTooling();
            }

            private IList<string> AllowedConfigurations { get; set; }

            private IList<string> AllowedPlatforms { get; set; }

            private DirectoryInfo RootDirectory { get; set; }

            private DirectoryInfo ObjectDirectory { get; set; }

            private DirectoryInfo BinariesDirectory { get; set; }

            private DirectoryInfo SolutionDirectory { get; set; }

            private DirectoryInfo ToolsDirectory { get; set; }

            private IDictionary<string, DirectoryInfo> Tools { get; set; }

            public void Clean()
            {
                this.RemoveDirectory(this.ObjectDirectory);
                this.RemoveDirectory(this.BinariesDirectory);
            }

            public IEnumerable<FileInfo> GetTestFiles()
            {
                return this.BinariesDirectory.GetFiles(""*.Facts.dll"", SearchOption.AllDirectories);
            }

            public IEnumerable<FileInfo> GetSolutionFiles()
            {
                return this.SolutionDirectory.GetFiles(""*.sln"", SearchOption.TopDirectoryOnly);
            }

            public DirectoryInfo GetToolDirectory(string toolName)
            {
                if (!this.Tools.ContainsKey(toolName))
                {
                    throw new Exception(string.Format(""Tool '{0}' cannot be found in '{1}'"", toolName, this.ToolsDirectory.FullName));
                }

                return this.Tools[toolName];
            }

            private void DiscoverTooling()
            {
                this.Tools = new Dictionary<string, DirectoryInfo>();

                foreach(var toolDirectory in this.ToolsDirectory.GetDirectories(""*"", SearchOption.TopDirectoryOnly))
                {
                    DirectoryInfo version = null;

                    foreach(var versionDirectory in toolDirectory.GetDirectories(""*"", SearchOption.TopDirectoryOnly))
                    {
                        if (version == null || string.Compare(versionDirectory.Name, version.Name, StringComparison.Ordinal) > 0)
                        {
                            version = versionDirectory;
                        }
                    }

                    if (version != null)
                    {
                        this.Tools.Add(toolDirectory.Name, version);
                    }
                }
            }

            private void RemoveDirectory(DirectoryInfo directory)
            {
                if (directory.Exists)
                {
                    Console.WriteLine(""  Removing directory '{0}'"", directory.FullName);
                    directory.Delete(true);
                }
            }

            private void SetDirectories()
            {
                this.RootDirectory = new DirectoryInfo("".."");
                this.ObjectDirectory = new DirectoryInfo(Path.Combine(this.RootDirectory.FullName, ""obj""));
                this.BinariesDirectory = new DirectoryInfo(Path.Combine(this.RootDirectory.FullName, ""bin""));
                this.SolutionDirectory = new DirectoryInfo(Path.Combine(this.RootDirectory.FullName, ""sln""));
                this.ToolsDirectory = new DirectoryInfo(Path.Combine(this.RootDirectory.FullName, ""tools""));
            }
        }
    }
    "
    Add-Type -TypeDefinition $BuildHelperType -Language CSharpVersion3

    $Global:BuildHelper = New-Object -TypeName Build.Helper -ArgumentList $Global:AllowedConfigurations, $Global:AllowedPlatforms
}


function IsValueInSet([string] $Value, [string[]] $Set) {
    :SetItemLoop foreach ($SetItem in $Set) {
        if ($Value -eq $SetItem)
        {
            return $True
        }
    }
    return $False
}