namespace EmbeddedFx.Facts.Support
{
    using System;
    using System.IO;

    internal sealed class TestAppDomain
    {
        public TestAppDomain(DirectoryInfo baseDirectory)
        {
            this.InstantiateNewAppDomain(baseDirectory);
        }

        public AppDomain AppDomain { get; private set; }

        public void Unload()
        {
            ActOnObject.IfNotNull(this.AppDomain, (ad) => AppDomain.Unload(ad));
        }

        private void InstantiateNewAppDomain(DirectoryInfo baseDirectory)
        {
            var appDomainSetup = new AppDomainSetup()
            {
                ApplicationBase = baseDirectory.FullName,
                ApplicationTrust = AppDomain.CurrentDomain.ApplicationTrust
            };

            this.AppDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, appDomainSetup);
        }
    }
}
