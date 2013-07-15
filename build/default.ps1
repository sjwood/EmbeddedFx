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


Task Clean -Depends InstantiateBuildHelper -Description "Cleans all build products" {
    $Global:BuildHelper.Clean()
}


Task ValidateProperties -Depends InstantiateBuildHelper -Description "Validates script properties" {
    if ($Global:BuildHelper.IsConfigurationAllowed($Configuration) -eq $False)
    {
        Throw "'$Configuration' is not an allowed configuration. Allowed configurations are $Global:AllowedConfigurations."
    }
    if ($Global:BuildHelper.IsPlatformAllowed($Platform) -eq $False)
    {
        Throw "'$Platform' is not an allowed platform. Allowed platforms are $Global:AllowedPlatforms."
    }
}


Task Build -Depends ValidateProperties, Clean, InstantiateBuildHelper -Description "Builds all code" {
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
                this.AllowedConfigurations = new List<string>(allowedConfigurations);
                this.AllowedPlatforms = new List<string>(allowedPlatforms);
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

            public bool IsConfigurationAllowed(string configuration)
            {
                return this.AllowedConfigurations.Contains(configuration);
            }

            public bool IsPlatformAllowed(string platform)
            {
                return this.AllowedPlatforms.Contains(platform);
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