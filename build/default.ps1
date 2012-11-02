# Copyright 2012 Steve Wood
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


$Framework = '4.0'
$RootDirectory = New-Object -TypeName System.IO.DirectoryInfo -ArgumentList ".."
$ObjectDirectory = New-Object -TypeName System.IO.DirectoryInfo -ArgumentList "$RootDirectory\obj"
$BinariesDirectory = New-Object -TypeName System.IO.DirectoryInfo -ArgumentList "$RootDirectory\bin"
$DocumentationDirectory = New-Object -TypeName System.IO.DirectoryInfo -ArgumentList "$RootDirectory\doc"


$Solutions = Get-ChildItem $RootDirectory.FullName -Include *.sln -Recurse


Task default -Depends Build


Task Clean -Description "Cleans all build products" {
    $Directories = @($ObjectDirectory, $BinariesDirectory, $DocumentationDirectory)
    :DirectoryLoop foreach ($Directory in $Directories) {
        if ($Directory.Exists)
        {
            Write-Output ("  Removing directory '" + $Directory.FullName + "'")
            Remove-Item $Directory.FullName -Force -Recurse
        }
    }
}

Task Build -Depends Clean -Description "Builds all code" {
    :SolutionLoop foreach ($Solution in $Solutions) {
        Write-Output ("  Building solution '" + $Solution.FullName + "'")
        Exec { MsBuild $Solution.FullName /nologo /verbosity:minimal /maxcpucount /p:Configuration=$Configuration /p:Platform=$Platform }
    }
}
