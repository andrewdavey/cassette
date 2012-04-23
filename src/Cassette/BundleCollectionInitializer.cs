using System.Collections.Generic;

namespace Cassette
{
    class BundleCollectionInitializer : IBundleCollectionInitializer
    {
        readonly IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations;
        readonly ExternalBundleGenerator externalBundleGenerator;
        BundleCollection bundles;

        public BundleCollectionInitializer(IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations, ExternalBundleGenerator externalBundleGenerator)
        {
            this.bundleConfigurations = bundleConfigurations;
            this.externalBundleGenerator = externalBundleGenerator;
        }

        public void Initialize(BundleCollection bundleCollection)
        {
            using (bundleCollection.GetWriteLock())
            {
                bundles = bundleCollection;
                bundles.Clear();
                AddBundles();
                ProcessBundles();
                AddBundlesForUrlReferences();
                bundles.BuildReferences();
            }
        }

        void AddBundles()
        {
            bundles.AddRange(bundleConfigurations);
        }

        void ProcessBundles()
        {
            bundles.Process();
        }

        void AddBundlesForUrlReferences()
        {
            externalBundleGenerator.AddBundlesForUrlReferences(bundles);
        }
    }
}