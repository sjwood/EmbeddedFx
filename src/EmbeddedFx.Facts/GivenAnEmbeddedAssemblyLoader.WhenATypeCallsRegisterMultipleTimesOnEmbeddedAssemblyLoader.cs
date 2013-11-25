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
        public sealed class WhenATypeCallsRegisterMultipleTimesOnEmbeddedAssemblyLoader
        {
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
                                EmbeddedAssemblyLoader.Register();
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
            public void FromMultipleTypesThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
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
                                static A()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class B
                            {
                                static B()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class C
                            {
                                static C()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
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
            public void FromMultipleThreadsThenAppDomainAssemblyResolveEventSubscriberCountShouldBeOne()
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
                        using System.Collections.Generic;
                        using System.Threading;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            private IList<A> InstancesOfA;

                            public " + testClassName + @"()
                            {
                                this.InstancesOfA = new List<A>();
                                new Thread(this.SleepForFiveHundredMilliseconds).Start();
                                new Thread(this.SleepForFiveHundredMilliseconds).Start();
                                new Thread(this.SleepForFiveHundredMilliseconds).Start();
                                Thread.Sleep(2000);
                            }

                            private void SleepForFiveHundredMilliseconds()
                            {
                                this.InstancesOfA.Add(new A());
                            }

                            private class A
                            {
                                static A()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                    Thread.Sleep(500);
                                }
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
            public void FromMultipleTypesAndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
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
                                static A()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class B
                            {
                                static B()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
                            }

                            private class C
                            {
                                static C()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                }
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
            public void FromMultipleThreadsAndHasItsOwnAssemblyResolveEventHandlerThenAppDomainAssemblyResolveEventSubscriberCountShouldBeTwo()
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
                        using System.Collections.Generic;
                        using System.Reflection;
                        using System.Threading;
                        using EmbeddedFx;

                        public class " + testClassName + @" : MarshalByRefObject
                        {
                            private IList<A> InstancesOfA;

                            public " + testClassName + @"()
                            {
                                this.InstancesOfA = new List<A>();
                                AppDomain.CurrentDomain.AssemblyResolve += " + testClassName + @".OwnAssemblyResolveEventHandler;
                                new Thread(this.CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(this.CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                new Thread(this.CallRegisterThenSleepForFiveHundredMilliseconds).Start();
                                Thread.Sleep(2000);
                            }

                            private static Assembly OwnAssemblyResolveEventHandler(object sender, ResolveEventArgs args)
                            {
                                return null;
                            }

                            private void CallRegisterThenSleepForFiveHundredMilliseconds()
                            {
                                this.InstancesOfA.Add(new A());
                            }

                            private class A
                            {
                                static A()
                                {
                                    EmbeddedAssemblyLoader.Register();
                                    Thread.Sleep(500);
                                }
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
        }
    }
}
