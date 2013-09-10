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
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class AppDomainInfoProvider : MarshalByRefObject
    {
        private static readonly IDictionary<Version, string> ClrVersionToFieldNameMap = new Dictionary<Version, string>()
        {
            { new Version(2, 0), "AssemblyResolve" },
            { new Version(4, 0), "_AssemblyResolve" }
        };

        public AssemblyName[] GetLoadedAssemblyNames()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyNames = new AssemblyName[assemblies.Length];

            for (var i = 0; i < assemblies.Length; i++)
            {
                assemblyNames[i] = assemblies[i].GetName();
            }

            return assemblyNames;
        }

        public int GetAssemblyResolveEventSubscriberCount()
        {
            var eventHandler = AppDomainInfoProvider.GetAppDomainAssemblyResolveEventHandler();
            if (eventHandler == null)
            {
                return 0;
            }

            var subscribers = eventHandler.GetInvocationList();
            if (subscribers == null)
            {
                return 0;
            }

            return subscribers.Length;
        }

        private static ResolveEventHandler GetAppDomainAssemblyResolveEventHandler()
        {
            var fieldName = GetAssemblyResolveEventHandlerFieldNameForExecutingClr();

            var field = typeof(AppDomain).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

            return (ResolveEventHandler)field.GetValue(AppDomain.CurrentDomain);
        }

        private static string GetAssemblyResolveEventHandlerFieldNameForExecutingClr()
        {
            var clrVersion = new Version(Environment.Version.Major, Environment.Version.Minor);

            if (!ClrVersionToFieldNameMap.ContainsKey(clrVersion))
            {
                throw new NotImplementedException("This is not supported in this version of the framework");
            }

            return ClrVersionToFieldNameMap[clrVersion];
        }
    }
}