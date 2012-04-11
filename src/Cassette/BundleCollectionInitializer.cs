using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    class BundleCollectionInitializer : IBundleCollectionInitializer
    {
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        readonly ExternalBundleGenerator externalBundleGenerator;
        BundleCollection bundles;

        public BundleCollectionInitializer(IEnumerable<IBundleDefinition> bundleDefinitions, ExternalBundleGenerator externalBundleGenerator)
        {
            this.bundleDefinitions = bundleDefinitions;
            this.externalBundleGenerator = externalBundleGenerator;
        }

        public void Initialize(BundleCollection bundleCollection)
        {
            using (bundleCollection.GetWriteLock())
            {
                bundles = bundleCollection;
                AddBundlesFromDefinitions();
                ProcessBundles();
                AddBundlesForUrlReferences();
                bundles.BuildReferences();
            }
        }

        void AddBundlesFromDefinitions()
        {
            bundles.AddRange(bundleDefinitions);
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