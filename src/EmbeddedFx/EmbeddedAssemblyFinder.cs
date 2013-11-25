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
    using System.IO;
    using System.Reflection;
    using Mono.Cecil;

    internal sealed class EmbeddedAssemblyFinder
    {
        private const int BufferSize = 32768;

        private static readonly Assembly MonoCecilAssembly;

        static EmbeddedAssemblyFinder()
        {
            EmbeddedAssemblyFinder.MonoCecilAssembly = EmbeddedAssemblyFinder.LoadMonoCecil();
        }

        public EmbeddedAssemblyFinder(AppDomain appDomain)
        {
            this.AppDomain = appDomain;
        }

        private AppDomain AppDomain { get; set; }

        public Assembly Find(AssemblyName name)
        {
            if (name.FullName == EmbeddedAssemblyFinder.MonoCecilAssembly.FullName)
            {
                return EmbeddedAssemblyFinder.MonoCecilAssembly;
            }

            var loadedAssemblies = this.AppDomain.GetAssemblies();

            foreach (var loadedAssembly in loadedAssemblies)
            {
                var embeddedResourcesBytes = EmbeddedAssemblyFinder.GetEmbeddedResourcesBytes(loadedAssembly);

                foreach (var embeddedResourceBytes in embeddedResourcesBytes)
                {
                    var embeddedAssemblyBytes = EmbeddedAssemblyFinder.GetEmbeddedAssemblyBytes(embeddedResourceBytes, name);

                    if (embeddedAssemblyBytes != null)
                    {
                        return Assembly.Load(embeddedAssemblyBytes);
                    }
                }
            }

            return null;
        }

        private static byte[] GetEmbeddedAssemblyBytes(byte[] bytes, AssemblyName name)
        {
            using (var stream = new MemoryStream(bytes))
            {
                try
                {
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream);

                    if (assemblyDefinition.FullName == name.FullName)
                    {
                        return bytes;
                    }

                    foreach (var moduleDefinition in assemblyDefinition.Modules)
                    {
                        foreach (var resource in moduleDefinition.Resources)
                        {
                            if (resource.ResourceType == ResourceType.Embedded)
                            {
                                var embeddedResource = (EmbeddedResource)resource;
                                {
                                    var embeddedResourceBytes = embeddedResource.GetResourceData();

                                    var embeddedAssemblyBytes = EmbeddedAssemblyFinder.GetEmbeddedAssemblyBytes(embeddedResourceBytes, name);

                                    if (embeddedAssemblyBytes != null)
                                    {
                                        return embeddedResourceBytes;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                }

                return null;
            }
        }

        private static IEnumerable<byte[]> GetEmbeddedResourcesBytes(Assembly assembly)
        {
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (EmbeddedAssemblyFinder.IsEmbeddedResource(assembly, resourceName))
                {
                    using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                    {
                        yield return EmbeddedAssemblyFinder.StreamToByteArray(resourceStream);
                    }
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

            return EmbeddedAssemblyFinder.HasFlag(ResourceLocation.Embedded, resourceLocation)
                && EmbeddedAssemblyFinder.HasFlag(ResourceLocation.ContainedInManifestFile, resourceLocation);
        }

        private static Assembly LoadMonoCecil()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mono.Cecil.dll"))
            {
                return Assembly.Load(EmbeddedAssemblyFinder.StreamToByteArray(stream));
            }
        }

        private static byte[] StreamToByteArray(Stream input)
        {
            var output = new MemoryStream();

            var buffer = new byte[EmbeddedAssemblyFinder.BufferSize];

            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }

            return output.ToArray();
        }
    }
}
