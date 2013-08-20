// Copyright 2012-2013 Steve Wood
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Documentation ignored at present time.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1639:FileHeaderMustHaveSummary", Justification = "Documentation ignored at present time.")]

namespace EmbeddedFx.Facts.Support
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Reflection;

    internal sealed class IsolatedStorageWrapper : IDisposable
    {
        private const BindingFlags PrivateInstancePropertyFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty;

        public IsolatedStorageWrapper(IsolatedStorageFile isolatedStorageFile)
        {
            if (isolatedStorageFile == null)
            {
                throw new ArgumentNullException("isolatedStorageFile");
            }

            this.IsolatedStorageFile = isolatedStorageFile;
        }

        public DirectoryInfo StorageDirectory
        {
            get
            {
                var rootDirectoryGetter = this.IsolatedStorageFile.GetType().GetProperty("RootDirectory", IsolatedStorageWrapper.PrivateInstancePropertyFlags);
                if (rootDirectoryGetter == null)
                {
                    return null;
                }

                var rootDirectory = rootDirectoryGetter.GetValue(this.IsolatedStorageFile, null) as string;

                return new DirectoryInfo(rootDirectory);
            }
        }

        private IsolatedStorageFile IsolatedStorageFile { get; set; }

        public void Dispose()
        {
            ActOnObject.IfNotNull(this.IsolatedStorageFile, (isf) => isf.Dispose());
        }

        public void Remove()
        {
            ActOnObject.IfNotNull(this.IsolatedStorageFile, (isf) => isf.Remove());
        }
    }
}
