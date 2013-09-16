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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
                {
                    // arrange
                    var testSource = @"
                        namespace " + testNamespace + @"
                        {
                            using System;
                            using EmbeddedFx;

                            public class " + testClassName + @" : MarshalByRefObject
                            {
                                public " + testClassName + @"()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }
                        }";
                    var testEmbeddedResources = new string[] { };
                    var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                    var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    // act
                    var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));
                    
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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                EmbeddedAssemblyLoader.Register();
                            }
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                EmbeddedAssemblyLoader.Register();
                                EmbeddedAssemblyLoader.Register();
                                EmbeddedAssemblyLoader.Register();
                            }
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                new " + testClassName + @".A();
                                new " + testClassName + @".B();
                                new " + testClassName + @".C();
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
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using System.Threading;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                new Thread(" + testClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + testClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + testClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                Thread.Sleep(100);
                            }

                            private static void CallRegisterThenSleepForFiveHundredMilliseconds()
                            {
                                EmbeddedAssemblyLoader.Register();
                                Thread.Sleep(500);
                            }
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using System.Reflection;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + testClassName + @".OwnAssemblyResolveEventHandler;
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + testClassName + @".OwnAssemblyResolveEventHandler;
                                EmbeddedAssemblyLoader.Register();
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }
                        }
                    }";
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + testClassName + @".OwnAssemblyResolveEventHandler;
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
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + testClassName + @".OwnAssemblyResolveEventHandler;
                                new " + testClassName + @".A();
                                new " + testClassName + @".B();
                                new " + testClassName + @".C();
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
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

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
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                // arrange
                var testSource = @"
                    namespace " + testNamespace + @"
                    {
                        using System;
                        using System.Reflection;
                        using System.Threading;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            public " + testClassName + @"()
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += " + testClassName + @".OwnAssemblyResolveEventHandler;
                                new Thread(" + testClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + testClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(" + testClassName + @".CallRegisterThenSleepForFiveHundredMilliseconds).Start();
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
                var testEmbeddedResources = new string[] { };
                var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                // act
                var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                // assert
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                this.AssertAppDomainHasAssemblyResolveEventSubscribers(2, ts.TestAppDomain);
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeDoesNotCallRegisterOnEmbeddedAssemblyLoaderAndRefersToATypeThatIsNotEmbeddedAsAResourceShouldThrow()
        {
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                FileInfo libraryBinary = null;
                try
                {
                    // arrange
                    var librarySource = @"
                            namespace " + testNamespace + @"
                            {
                                public class LibraryType
                                {
                                }
                            }";
                    var libraryEmbeddedResources = new string[] { };
                    var libraryReferencedAssemblies = new string[] { };
                    libraryBinary = this.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                    var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    public " + testClassName + @"()
                                    {
                                        new LibraryType();
                                    }
                                }
                            }";
                    var testEmbeddedResources = new string[] { };
                    var testReferencedAssemblies = new string[] { "System.dll", libraryBinary.FullName };
                    var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    Exception thrownException = null;

                    // act
                    try
                    {
                        var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));
                    }
                    catch (TargetInvocationException tie)
                    {
                        thrownException = tie;
                    }
                    catch (Exception e)
                    {
                        Assert.True(false, string.Format("An unexpected exception occurred: {0}", e.Message));
                    }

                    // assert
                    Assert.NotNull(thrownException);
                    Assert.NotNull(thrownException.InnerException);
                    Assert.Equal(typeof(FileNotFoundException), thrownException.InnerException.GetType());
                    Assert.Equal(string.Format("Could not load file or assembly '{0}' or one of its dependencies. The system cannot find the file specified.", AssemblyName.GetAssemblyName(libraryBinary.FullName)), thrownException.InnerException.Message);
                }
                finally
                {
                    ActOnObject.IfNotNull(
                        libraryBinary,
                        (fi) =>
                        {
                            if (fi.Exists)
                            {
                                fi.Delete();
                            };
                        });
                }
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeDoesNotCallRegisterOnEmbeddedAssemblyLoaderAndRefersToATypeThatIsEmbeddedAsAResourceShouldThrow()
        {
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                FileInfo libraryBinary = null;
                try
                {
                    // arrange
                    var librarySource = @"
                            namespace " + testNamespace + @"
                            {
                                public class LibraryType
                                {
                                }
                            }";
                    var libraryEmbeddedResources = new string[] { };
                    var libraryReferencedAssemblies = new string[] { };
                    libraryBinary = this.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                    var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    public " + testClassName + @"()
                                    {
                                        new LibraryType();
                                    }
                                }
                            }";
                    var testEmbeddedResources = new string[] { libraryBinary.FullName };
                    var testReferencedAssemblies = new string[] { "System.dll", libraryBinary.FullName };
                    var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    Exception thrownException = null;

                    // act
                    try
                    {
                        var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));
                    }
                    catch (TargetInvocationException tie)
                    {
                        thrownException = tie;
                    }
                    catch (Exception e)
                    {
                        Assert.True(false, string.Format("An unexpected exception occurred: {0}", e.Message));
                    }

                    // assert
                    Assert.NotNull(thrownException);
                    Assert.NotNull(thrownException.InnerException);
                    Assert.Equal(typeof(FileNotFoundException), thrownException.InnerException.GetType());
                    Assert.Equal(string.Format("Could not load file or assembly '{0}' or one of its dependencies. The system cannot find the file specified.", AssemblyName.GetAssemblyName(libraryBinary.FullName)), thrownException.InnerException.Message);
                }
                finally
                {
                    ActOnObject.IfNotNull(
                        libraryBinary,
                        (fi) =>
                        {
                            if (fi.Exists)
                            {
                                fi.Delete();
                            };
                        });
                }
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterOnEmbeddedAssemblyLoaderAndRefersToATypeThatIsNotEmbeddedAsAResourceShouldThrow()
        {
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                FileInfo libraryBinary = null;
                try
                {
                    // arrange
                    var librarySource = @"
                            namespace " + testNamespace + @"
                            {
                                public class LibraryType
                                {
                                }
                            }";
                    var libraryEmbeddedResources = new string[] { };
                    var libraryReferencedAssemblies = new string[] { };
                    libraryBinary = this.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                    var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
                                using EmbeddedFx;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    public " + testClassName + @"()
                                    {
                                        EmbeddedAssemblyLoader.Register();
                                        new LibraryType();
                                    }
                                }
                            }";
                    var testEmbeddedResources = new string[] { };
                    var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll", libraryBinary.FullName };
                    var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    Exception thrownException = null;

                    // act
                    try
                    {
                        var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));
                    }
                    catch (TargetInvocationException tie)
                    {
                        thrownException = tie;
                    }
                    catch (Exception e)
                    {
                        Assert.True(false, string.Format("An unexpected exception occurred: {0}", e.Message));
                    }

                    // assert
                    Assert.NotNull(thrownException);
                    Assert.NotNull(thrownException.InnerException);
                    Assert.Equal(typeof(FileNotFoundException), thrownException.InnerException.GetType());
                    Assert.Equal(string.Format("Could not load file or assembly '{0}' or one of its dependencies. The system cannot find the file specified.", AssemblyName.GetAssemblyName(libraryBinary.FullName)), thrownException.InnerException.Message);
                }
                finally
                {
                    ActOnObject.IfNotNull(
                        libraryBinary,
                        (fi) =>
                        {
                            if (fi.Exists)
                            {
                                fi.Delete();
                            };
                        });
                }
            };

            this.ExecuteTestInsideTestAppDomain(testSetup);
        }

        [Fact(Skip = "Implementation missing in EmbeddedAssemblyLoader")]
        [Trait("Assembly", "EmbeddedFx")]
        public void WhenATypeCallsRegisterOnEmbeddedAssemblyLoaderAndRefersToATypeThatIsEmbeddedAsAResourceThenEmbeddedResourceAssemblyShouldBeLoadedIntoAppDomain()
        {
            var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            var testClassName = MethodBase.GetCurrentMethod().Name;

            Action<TestSetup> testSetup = (ts) =>
            {
                FileInfo libraryBinary = null;
                try
                {
                    // arrange
                    var librarySource = @"
                            namespace " + testNamespace + @"
                            {
                                public class LibraryType
                                {
                                }
                            }";
                    var libraryEmbeddedResources = new string[] { };
                    var libraryReferencedAssemblies = new string[] { };
                    libraryBinary = this.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                    var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
                                using EmbeddedFx;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    public " + testClassName + @"()
                                    {
                                        EmbeddedAssemblyLoader.Register();
                                        new LibraryType();
                                    }
                                }
                            }";
                    var testEmbeddedResources = new string[] { libraryBinary.FullName };
                    var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll", libraryBinary.FullName };
                    var testBinary = this.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    // act
                    var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                    // assert
                    var libraryBinaryAssemblyName = AssemblyName.GetAssemblyName(libraryBinary.FullName);
                    this.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, libraryBinaryAssemblyName);
                    this.AssertAppDomainHasLoadedAssemblyName(true, ts.TestAppDomain, libraryBinaryAssemblyName);
                }
                finally
                {
                    ActOnObject.IfNotNull(
                        libraryBinary,
                        (fi) =>
                        {
                            if (fi.Exists)
                            {
                                fi.Delete();
                            };
                        });
                }
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

        private FileInfo CompileCodeIntoLocation(string source, IEnumerable<string> embeddedResources, IEnumerable<string> referencedAssemblies, DirectoryInfo location)
        {
            if (!location.Exists)
            {
                throw new ArgumentException(string.Format("Directory '{0}' does not exist", location.FullName), "location");
            }

            var binaryPath = Path.Combine(location.FullName, string.Format("{0}.dll", Guid.NewGuid()));

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