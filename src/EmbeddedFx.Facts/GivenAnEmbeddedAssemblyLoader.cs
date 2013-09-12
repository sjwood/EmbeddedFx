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

    public class GivenAnEmbeddedAssemblyLoader
    {
        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeDoesNotCallRegisterOnEmbeddedAssemblyLoaderThenEmbeddedFxShouldNotBeLoadedIntoAppDomain()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                var embeddedFxAssemblyName = AssemblyName.GetAssemblyName(new FileInfo("EmbeddedFx.dll").FullName);
                this.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, embeddedFxAssemblyName);
                this.AssertAppDomainHasLoadedAssemblyName(false, ts.TestAppDomain, embeddedFxAssemblyName);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterOnEmbeddedAssemblyLoaderThenEmbeddedFxShouldBeLoadedIntoAppDomain()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
                {
                    // arrange
                    var source = @"
                        namespace " + sourceNamespace + @"
                        {
                            using System;
                            using EmbeddedFx;

                            public class " + sourceClassName + @" : MarshalByRefObject
                            {
                                public " + sourceClassName + @"()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }
                        }";
                    var embeddedResources = new string[] { };
                    var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                    var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                    // act
                    var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));
                    
                    // assert
                    var embeddedFxAssemblyName = AssemblyName.GetAssemblyName(new FileInfo("EmbeddedFx.dll").FullName);
                    this.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, embeddedFxAssemblyName);
                    this.AssertAppDomainHasLoadedAssemblyName(true, ts.TestAppDomain, embeddedFxAssemblyName);
                };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeDoesNotCallRegisterOnEmbeddedAssemblyLoaderThenAppDomainAssemblyResolveEventSubscriberCountShouldBeZero()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterOnEmbeddedAssemblyLoaderThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                EmbeddedAssemblyLoader.Register();
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(1, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoaderThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                EmbeddedAssemblyLoader.Register();
                                EmbeddedAssemblyLoader.Register();
                                EmbeddedAssemblyLoader.Register();
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(1, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoaderFromMultipleTypesThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                new " + sourceClassName + @".A();
                                new " + sourceClassName + @".B();
                                new " + sourceClassName + @".C();
                            }

                            private class A
                            {
                                public A()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class B
                            {
                                public B()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class C
                            {
                                public C()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(1, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoaderFromMultipleThreadsThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using System.Threading;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                new Thread(" + sourceClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + sourceClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + sourceClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                Thread.Sleep(100);
                            }

                            private static void CallRegisterThenSleepForFiveHundredMilliseconds()
                            {
                                EmbeddedAssemblyLoader.Register();
                                Thread.Sleep(500);
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(1, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeDoesNotCallRegisterOnEmbeddedAssemblyLoaderButHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using System.Reflection;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + sourceClassName + @".OwnAssemblyResolveEventHandler;
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(1, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterOnEmbeddedAssemblyLoaderAndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + sourceClassName + @".OwnAssemblyResolveEventHandler;
                                EmbeddedAssemblyLoader.Register();
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(2, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoaderAndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + sourceClassName + @".OwnAssemblyResolveEventHandler;
                                EmbeddedAssemblyLoader.Register();
                                EmbeddedAssemblyLoader.Register();
                                EmbeddedAssemblyLoader.Register();
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(2, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoaderFromMultipleTypesAndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + sourceClassName + @".OwnAssemblyResolveEventHandler;
                                new " + sourceClassName + @".A();
                                new " + sourceClassName + @".B();
                                new " + sourceClassName + @".C();
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }

                            private class A
                            {
                                public A()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class B
                            {
                                public B()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class C
                            {
                                public C()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(2, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoaderFromMultipleThreadsAndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
        {
            var sourceNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var sourceClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var source = @"
                    namespace " + sourceNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using System.Threading;
                        using EmbeddedFx;

                        public class " + sourceClassName + @" : MarshalByRefObject
                        {
                            public " + sourceClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + sourceClassName + @".OwnAssemblyResolveEventHandler;
                                new Thread(" + sourceClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + sourceClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + sourceClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                Thread.Sleep(100);
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }

                            private static void CallRegisterThenSleepForFiveHundredMilliseconds()
                            {
                                EmbeddedAssemblyLoader.Register();
                                Thread.Sleep(500);
                            }
                        }
                    }";
                var embeddedResources = new string[] { };
                var referencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoAppDomainsPath(ts.TestAppDomain, source, embeddedResources, referencedAssemblies);

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", sourceNamespace, sourceClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(2, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        private void ExecuteTestInsideTestAppDomain(Action<TestSetup> action)
        {
            IsolatedStorageWrapper isolatedStorageWrapper = null;
            TestAppDomain testAppDomain = null;

            try
            {
                isolatedStorageWrapper = new IsolatedStorageWrapper(IsolatedStorageFile.GetUserStoreForAssembly());

                this.CopyExecutingAssemblyTo(isolatedStorageWrapper.StorageDirectory);

                testAppDomain = new TestAppDomain(isolatedStorageWrapper.StorageDirectory);

                action(new TestSetup(AppDomain.CurrentDomain, testAppDomain.AppDomain));
            }
            finally
            {
                ActOnObject.IfNotNull(testAppDomain, (tad) => tad.Unload());
                ActOnObject.IfNotNull(isolatedStorageWrapper, (sw) => sw.Remove());
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

        private void AssertAppDomainHasAssemblyResolveEventSubscribers(int expected, AppDomain appDomain)
        {
            var proxy = (AppDomainInfoProvider)appDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AppDomainInfoProvider).FullName);
            var subscriberCount = proxy.GetAssemblyResolveEventSubscriberCount();

            Assert.Equal(expected, subscriberCount);
        }

        private FileInfo CompileCodeIntoAppDomainsPath(AppDomain appDomain, string source, IEnumerable<string> embeddedResources, IEnumerable<string> referencedAssemblies)
        {
            var binaryPath = Path.Combine(appDomain.BaseDirectory, string.Format("{0}.dll", Guid.NewGuid()));

            CSharpCompiler.Compile(source, binaryPath, embeddedResources, referencedAssemblies);

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