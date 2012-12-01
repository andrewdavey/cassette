using System;

namespace Cassette.RequireJS
{
    public class BundleCollectionExtensionsUtilities : IStartUpTask
    {
        readonly Func<IModuleInitializer> createAmdConfiguration;

        public BundleCollectionExtensionsUtilities(Func<IModuleInitializer> createAmdConfiguration)
        {
            this.createAmdConfiguration = createAmdConfiguration;
        }

        public void Start()
        {
            BundleCollectionExtensions.CreateAmdConfiguration = createAmdConfiguration;
        }
    }
}