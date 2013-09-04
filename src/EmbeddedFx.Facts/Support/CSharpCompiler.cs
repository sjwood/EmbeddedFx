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
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.CSharp;

    internal static class CSharpCompiler
    {
        public static void Compile(string source, string outputAssembly, bool generateExecutable, params string[] referencedAssemblies)
        {
            using (var provider = new CSharpCodeProvider())
            {
                var parameters = CSharpCompiler.GetCompilerParameters(outputAssembly, generateExecutable);

                CSharpCompiler.AddReferencedAssemblies(parameters, referencedAssemblies);

                var results = provider.CompileAssemblyFromSource(parameters, source);

                if (results.Errors.HasErrors)
                {
                    var message = CSharpCompiler.GenerateExceptionMessage(results.Errors);

                    throw new Exception(message);
                }
            }
        }

        private static CompilerParameters GetCompilerParameters(string outputAssembly, bool generateExecutable)
        {
            return new CompilerParameters()
            {
                TreatWarningsAsErrors = true,
                WarningLevel = 4,
                GenerateInMemory = false,
                IncludeDebugInformation = false,
                OutputAssembly = outputAssembly,
                GenerateExecutable = generateExecutable
            };
        }

        private static void AddReferencedAssemblies(CompilerParameters parameters, params string[] referencedAssemblies)
        {
            foreach (var referencedAssembly in referencedAssemblies)
            {
                parameters.ReferencedAssemblies.Add(referencedAssembly);
            }
        }

        private static string GenerateExceptionMessage(CompilerErrorCollection errors)
        {
            var builder = new StringBuilder();

            if (errors.HasErrors)
            {
                builder.Append("Compilation errors:\r\n");

                for (var i = 0; i < errors.Count; i++)
                {
                    builder.Append(errors[i]);
                    if (i < (errors.Count - 1))
                    {
                        builder.Append("\r\n");
                    }
                }
            }

            return builder.ToString();
        }
    }
}
