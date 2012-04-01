using System.Collections.Generic;
using Cassette.Configuration;

namespace Cassette
{
    class DebugModeBundleCollectionBuilder : IBundleCollectionBuilder
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
            AddBundlesFromDefinitions();
            ProcessBundles();
            AddBundlesForUrlReferences();
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