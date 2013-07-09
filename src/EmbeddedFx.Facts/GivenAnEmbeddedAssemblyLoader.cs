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
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Reflection;
    using EmbeddedFx.Facts.Support;
    using Xunit;

    public class GivenAnEmbeddedAssemblyLoader
    {
        //// TODO
        //// what happens if I call Dispose multiple times on the isolatedstoragewrapper????

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenAnAssemblyDoesNotReferToEmbeddedAssemblyLoaderThenEmbeddedFxShouldNotBeLoadedIntoAppDomain()
        {
            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace GivenAnEmbeddedAssemblyLoader
                    {
                        public class WhenAnAssemblyRefersToEmbeddedAssemblyLoaderThenEmbeddedFxShouldBeLoadedIntoAppDomain
                        {
                            public static int Main(string[] args)
                            {
                                return 0;
                            }
                        }
                    }";
                var embeddedFxFileInfo = new FileInfo(".\\EmbeddedFx.dll");
                var testExecutable = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, true, "System.dll", embeddedFxFileInfo.Name);

                // act
                var exitCode = ts.TestAppDomain.ExecuteAssembly(testExecutable.FullName);

                // assert
                Assert.Equal(0, exitCode);
                var embeddedFxAssemblyName = AssemblyName.GetAssemblyName(embeddedFxFileInfo.FullName);
                this.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, embeddedFxAssemblyName);
                this.AssertAppDomainHasLoadedAssemblyName(false, ts.TestAppDomain, embeddedFxAssemblyName);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenAnAssemblyInstantiatesEmbeddedAssemblyLoaderThenEmbeddedFxShouldBeLoadedIntoAppDomain()
        {
            Action<TestSetup> testSetup = (ts) =>
                {
                    // arrange
                    var source = @"
                        namespace GivenAnEmbeddedAssemblyLoader
                        {
                            using EmbeddedFx;

                            public class WhenAnAssemblyRefersToEmbeddedAssemblyLoaderThenEmbeddedFxShouldBeLoadedIntoAppDomain
                            {
                                private static EmbeddedAssemblyLoader _EmbeddedAssemblyLoader;

                                static WhenAnAssemblyRefersToEmbeddedAssemblyLoaderThenEmbeddedFxShouldBeLoadedIntoAppDomain()
                                {
                                    _EmbeddedAssemblyLoader = new EmbeddedAssemblyLoader();
                                }

                                public static int Main(string[] args)
                                {
                                    return 0;
                                }
                            }
                        }";
                    var embeddedFxFileInfo = new FileInfo(".\\EmbeddedFx.dll");
                    var testExecutable = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, true, "System.dll", embeddedFxFileInfo.Name);

                    // act
                    var exitCode = ts.TestAppDomain.ExecuteAssembly(testExecutable.FullName);
                    
                    // assert
                    Assert.Equal(0, exitCode);
                    var embeddedFxAssemblyName = AssemblyName.GetAssemblyName(embeddedFxFileInfo.FullName);
                    this.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, embeddedFxAssemblyName);
                    this.AssertAppDomainHasLoadedAssemblyName(true, ts.TestAppDomain, embeddedFxAssemblyName);
                };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        private void ExecuteTestInsideTestAppDomain(Action<TestSetup> action)
        {
            IsolatedStorageWrapper storageWrapper = null;
            TestAppDomain testAppDomain = null;

            try
            {
                storageWrapper = new IsolatedStorageWrapper(IsolatedStorageFile.GetUserStoreForAssembly());

                this.CopyExecutingAssemblyTo(storageWrapper.StorageDirectory);

                testAppDomain = new TestAppDomain(storageWrapper.StorageDirectory);

                action(new TestSetup(AppDomain.CurrentDomain, testAppDomain.AppDomain));
            }
            finally
            {
                ActOnObject.IfNotNull(testAppDomain, (tad) => tad.Unload());
                ActOnObject.IfNotNull(storageWrapper, (sw) => sw.Remove());
            }
        }

        private void AssertAppDomainHasLoadedAssemblyName(bool expected, AppDomain appDomain, AssemblyName assemblyName)
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

        private FileInfo CompileCodeIntoAppDomainsPath(AppDomain appDomain, string source, bool generateExecutable, params string[] referencedAssemblies)
        {
            var fileExtension = generateExecutable
                ? "exe"
                : "dll";

            var binaryPath = Path.Combine(appDomain.BaseDirectory, string.Format("{0}.{1}", Guid.NewGuid(), fileExtension));

            CSharpCompiler.Compile(source, binaryPath, generateExecutable, referencedAssemblies);

            return new FileInfo(binaryPath);
        }

        private void CopyExecutingAssemblyTo(DirectoryInfo to)
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