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

namespace EmbeddedFx.Facts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Reflection;
    using EmbeddedFx.Facts.Support;
    using Xunit;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Documentation ignored at present time.")]
    public sealed partial class GivenAnEmbeddedAssemblyLoader
    {
        internal static void ExecuteTestInsideTestAppDomain(Action<TestSetup> action)
        {
            IsolatedStorageWrapper isolatedStorageWrapper = null;
            TestAppDomain testAppDomain = null;

            try
            {
                isolatedStorageWrapper = new IsolatedStorageWrapper(IsolatedStorageFile.GetUserStoreForAssembly());

                GivenAnEmbeddedAssemblyLoader.CopyExecutingAssemblyTo(isolatedStorageWrapper.StorageDirectory);

                testAppDomain = new TestAppDomain(isolatedStorageWrapper.StorageDirectory);

                action(new TestSetup(AppDomain.CurrentDomain, testAppDomain.AppDomain));
            }
            finally
            {
                ActOnObject.IfNotNull(testAppDomain, (tad) => tad.Unload());
                ActOnObject.IfNotNull(isolatedStorageWrapper, (sw) => sw.Remove());
            }
        }

        internal static void AssertAppDomainHasLoadedAssemblyName(bool expected, AppDomain appDomain, AssemblyName assemblyName)
        {
            var hasLoaded = false;

            var proxy = (AppDomainInfoProvider)appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AppDomainInfoProvider).FullName);
            var names = proxy.GetLoadedAssemblyNames();

            foreach (var name in names)
            {
                if (name.FullName == assemblyName.FullName)
                {
                    hasLoaded = true;
                    break;
                }
            }

            var notOrEmpty = expected ? " NOT" : string.Empty;
            var message = string.Format("AppDomain '{0}' has incorrectly{1} loaded Assembly '{2}'", appDomain.FriendlyName, notOrEmpty, assemblyName.FullName);

            Assert.True(expected == hasLoaded, string.Format("Has{0} loaded {1}!", hasLoaded ? string.Empty : " NOT", assemblyName.FullName));
        }

        internal static void AssertAppDomainHasAssemblyResolveEventSubscribers(int expected, AppDomain appDomain)
        {
            var proxy = (AppDomainInfoProvider)appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AppDomainInfoProvider).FullName);
            var subscriberCount = proxy.GetAssemblyResolveEventSubscriberCount();

            Assert.Equal(expected, subscriberCount);
        }

        internal static FileInfo CompileCodeIntoLocation(string source, IEnumerable<string> embeddedResources, IEnumerable<string> referencedAssemblies, DirectoryInfo location)
        {
            if (!location.Exists)
            {
                throw new ArgumentException(string.Format("Directory '{0}' does not exist", location.FullName), "location");
            }

            var binaryPath = Path.Combine(location.FullName, string.Format("{0}.dll", Guid.NewGuid()));

            CSharpCompiler.Compile(source, binaryPath, embeddedResources, referencedAssemblies);

            return new FileInfo(binaryPath);
        }

        internal static void CopyExecutingAssemblyTo(DirectoryInfo to)
        {
            var executingAssemblyDirectory = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Directory;

            var assemblies = executingAssemblyDirectory.GetFiles("*.dll");

            foreach (var assembly in assemblies)
            {
                File.Copy(assembly.FullName, Path.Combine(to.FullName, assembly.Name));
            }
        }
    }
}