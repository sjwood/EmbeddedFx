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
    using System.Reflection;
    using EmbeddedFx.Facts.Support;
    using Xunit;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Documentation ignored at present time.")]
    public sealed partial class GivenAnEmbeddedAssemblyLoader
    {
        public sealed class WhenATypeCallsRegisterOnEmbeddedAssemblyLoader
        {
            [Fact]
            [Trait("Assembly", "EmbeddedFx")]
            public void ThenEmbeddedFxShouldBeLoadedIntoAppDomain()
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
                                static " + testClassName + @"()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }
                        }";
                    var testEmbeddedResources = new string[] { };
                    var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                    var testBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    // act
                    var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                    // assert
                    var embeddedFxAssemblyName = AssemblyName.GetAssemblyName(new FileInfo("EmbeddedFx.dll").FullName);
                    GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, embeddedFxAssemblyName);
                    GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(true, ts.TestAppDomain, embeddedFxAssemblyName);
                };

                GivenAnEmbeddedAssemblyLoader.ExecuteTestInsideTestAppDomain(testSetup);
            }

            [Fact]
            [Trait("Assembly", "EmbeddedFx")]
            public void ThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
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
                            static " + testClassName + @"()
                            {
                                EmbeddedAssemblyLoader.Register();
                            }
                        }
                    }";
                    var testEmbeddedResources = new string[] { };
                    var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll" };
                    var testBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    // act
                    var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                    // assert
                    GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                    GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasAssemblyResolveEventSubscribers(1, ts.TestAppDomain);
                };

                GivenAnEmbeddedAssemblyLoader.ExecuteTestInsideTestAppDomain(testSetup);
            }

            [Fact]
            [Trait("Assembly", "EmbeddedFx")]
            public void AndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
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
                            static " + testClassName + @"()
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
                    var testBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                    // act
                    var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                    // assert
                    GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasAssemblyResolveEventSubscribers(0, ts.ParentAppDomain);
                    GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasAssemblyResolveEventSubscribers(2, ts.TestAppDomain);
                };

                GivenAnEmbeddedAssemblyLoader.ExecuteTestInsideTestAppDomain(testSetup);
            }

            [Fact]
            [Trait("Assembly", "EmbeddedFx")]
            public void AndRefersToATypeThatIsNotEmbeddedAsAResourceShouldThrow()
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
                        libraryBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                        var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
                                using EmbeddedFx;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    static " + testClassName + @"()
                                    {
                                        EmbeddedAssemblyLoader.Register();
                                    }

                                    public " + testClassName + @"()
                                    {
                                        new LibraryType();
                                    }
                                }
                            }";
                        var testEmbeddedResources = new string[] { };
                        var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll", libraryBinary.FullName };
                        var testBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

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
                        ActOnObject.IfNotNull(libraryBinary, (fi) => fi.Delete());
                    }
                };

                GivenAnEmbeddedAssemblyLoader.ExecuteTestInsideTestAppDomain(testSetup);
            }

            [Fact]
            [Trait("Assembly", "EmbeddedFx")]
            public void AndRefersToATypeThatIsEmbeddedAsAResourceThenEmbeddedResourceAssemblyShouldBeLoadedIntoAppDomain()
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
                        libraryBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                        var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
                                using EmbeddedFx;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    static " + testClassName + @"()
                                    {
                                        EmbeddedAssemblyLoader.Register();
                                    }

                                    public " + testClassName + @"()
                                    {
                                        new LibraryType();
                                    }
                                }
                            }";
                        var testEmbeddedResources = new string[] { libraryBinary.FullName };
                        var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll", libraryBinary.FullName };
                        var testBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                        // act
                        var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                        // assert
                        var libraryBinaryAssemblyName = AssemblyName.GetAssemblyName(libraryBinary.FullName);
                        GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, libraryBinaryAssemblyName);
                        GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(true, ts.TestAppDomain, libraryBinaryAssemblyName);
                    }
                    finally
                    {
                        ActOnObject.IfNotNull(libraryBinary, (fi) => fi.Delete());
                    }
                };

                GivenAnEmbeddedAssemblyLoader.ExecuteTestInsideTestAppDomain(testSetup);
            }

            [Fact]
            [Trait("Assembly", "EmbeddedFx")]
            public void AndRefersToATypeThatIsEmbeddedAsANestedEmbeddedResourceThenEmbeddedResourceAssemblyShouldBeLoadedIntoAppDomain()
            {
                var testNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
                var testClassName = MethodBase.GetCurrentMethod().Name;

                Action<TestSetup> testSetup = (ts) =>
                {
                    FileInfo nestedLibraryBinary = null;
                    FileInfo libraryBinary = null;
                    try
                    {
                        // arrange
                        var nestedLibrarySource = @"
                            namespace " + testNamespace + @"
                            {
                                public class NestedLibraryType
                                {
                                }
                            }";
                        var nestedLibraryEmbeddedResources = new string[] { };
                        var nestedLibraryReferencedAssemblies = new string[] { };
                        nestedLibraryBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(nestedLibrarySource, nestedLibraryEmbeddedResources, nestedLibraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                        var librarySource = @"
                            namespace " + testNamespace + @"
                            {
                                public class LibraryType
                                {
                                }
                            }";
                        var libraryEmbeddedResources = new string[] { nestedLibraryBinary.FullName };
                        var libraryReferencedAssemblies = new string[] { };
                        libraryBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(librarySource, libraryEmbeddedResources, libraryReferencedAssemblies, new DirectoryInfo(Path.GetTempPath()));

                        var testSource = @"
                            namespace " + testNamespace + @"
                            {
                                using System;
                                using EmbeddedFx;
 
                                public class " + testClassName + @" : MarshalByRefObject
                                {
                                    static " + testClassName + @"()
                                    {
                                        EmbeddedAssemblyLoader.Register();
                                    }

                                    public " + testClassName + @"()
                                    {
                                        new NestedLibraryType();
                                    }
                                }
                            }";
                        var testEmbeddedResources = new string[] { libraryBinary.FullName };
                        var testReferencedAssemblies = new string[] { "System.dll", "EmbeddedFx.dll", libraryBinary.FullName, nestedLibraryBinary.FullName };
                        var testBinary = GivenAnEmbeddedAssemblyLoader.CompileCodeIntoLocation(testSource, testEmbeddedResources, testReferencedAssemblies, new DirectoryInfo(ts.TestAppDomain.BaseDirectory));

                        // act
                        var proxy = ts.TestAppDomain.CreateInstanceFromAndUnwrap(testBinary.FullName, string.Format("{0}.{1}", testNamespace, testClassName));

                        // assert
                        var libraryBinaryAssemblyName = AssemblyName.GetAssemblyName(libraryBinary.FullName);
                        GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, libraryBinaryAssemblyName);
                        GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(false, ts.TestAppDomain, libraryBinaryAssemblyName);

                        var nestedLibraryBinaryAssemblyName = AssemblyName.GetAssemblyName(nestedLibraryBinary.FullName);
                        GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(false, ts.ParentAppDomain, nestedLibraryBinaryAssemblyName);
                        GivenAnEmbeddedAssemblyLoader.AssertAppDomainHasLoadedAssemblyName(true, ts.TestAppDomain, nestedLibraryBinaryAssemblyName);
                    }
                    finally
                    {
                        ActOnObject.IfNotNull(libraryBinary, (fi) => fi.Delete());
                        ActOnObject.IfNotNull(nestedLibraryBinary, (fi) => fi.Delete());
                    }
                };

                GivenAnEmbeddedAssemblyLoader.ExecuteTestInsideTestAppDomain(testSetup);
            }
        }
    }
}
