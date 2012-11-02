namespace EmbeddedFx
{
    using System;

    public sealed class EmbeddedAssemblyLoader
    {
        public EmbeddedAssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => null;
        }
    }
}
