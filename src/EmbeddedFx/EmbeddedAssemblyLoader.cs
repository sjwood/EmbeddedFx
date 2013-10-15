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

namespace EmbeddedFx
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class EmbeddedAssemblyLoader
    {
        private static readonly object Padlock;

        static EmbeddedAssemblyLoader()
        {
            EmbeddedAssemblyLoader.Padlock = new object();
            EmbeddedAssemblyLoader.AlreadyRegistered = false;
        }

        private static bool AlreadyRegistered { get; set; }

        public static void Register()
        {
            lock (EmbeddedAssemblyLoader.Padlock)
            {
                if (EmbeddedAssemblyLoader.AlreadyRegistered)
                {
                    return;
                }

                AppDomain.CurrentDomain.AssemblyResolve += EmbeddedAssemblyLoader.AssemblyResolveEventHandler;

                EmbeddedAssemblyLoader.AlreadyRegistered = true;
            }
        }

        private static Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var appDomainAssemblies = (sender as AppDomain).GetAssemblies();

            var embeddedResources = EmbeddedAssemblyLoader.GetAllEmbeddedResources(appDomainAssemblies);

            //// TODO
            //// inspect the embedded resources to find the required assembly

            return null;
        }

        private static IEnumerable<EmbeddedResource> GetAllEmbeddedResources(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var embeddedResource in EmbeddedAssemblyLoader.GetEmbeddedResources(assembly))
                {
                    yield return embeddedResource;
                }
            }
        }

        private static IEnumerable<EmbeddedResource> GetEmbeddedResources(Assembly assembly)
        {
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (EmbeddedAssemblyLoader.IsEmbeddedResource(assembly, resourceName))
                {
                    yield return new EmbeddedResource(assembly, resourceName);
                }
            }
        }

        private static bool HasFlag(ResourceLocation flag, ResourceLocation value)
        {
            return (value & flag) == flag;
        }

        private static bool IsEmbeddedResource(Assembly assembly, string resourceName)
        {
            var resourceLocation = assembly.GetManifestResourceInfo(resourceName).ResourceLocation;

            return EmbeddedAssemblyLoader.HasFlag(ResourceLocation.Embedded, resourceLocation)
                && EmbeddedAssemblyLoader.HasFlag(ResourceLocation.ContainedInManifestFile, resourceLocation);
        }
    }
}
