using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    class DebugModeBundleCollectionBuilder
    {
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        readonly ExternalBundleGenerator externalBundleGenerator;
        BundleCollection bundles;

        public DebugModeBundleCollectionBuilder(IEnumerable<IBundleDefinition> bundleDefinitions, ExternalBundleGenerator externalBundleGenerator)
        {
            this.bundleDefinitions = bundleDefinitions;
            this.externalBundleGenerator = externalBundleGenerator;
        }

        public void BuildBundleCollection(BundleCollection bundleCollection)
        {
            bundles = bundleCollection;
            using (bundles.GetWriteLock())
            {
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