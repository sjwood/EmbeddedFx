[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Reviewed. Suppression is OK here.")]

namespace EmbeddedFx
{
    using System;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class EmbeddedAssemblyLoader
    {
        public EmbeddedAssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => null;
        }
    }
}
