using Cassette.Configuration;

namespace Cassette
{
    class BundleCollectionBuilder
    {
        readonly ProductionModeBundleCollectionBuilder productionModeBundleCollectionBuilder;
        readonly DebugModeBundleCollectionBuilder debugModeBundleCollectionBuilder;
        readonly PrecompiledBundleCollectionBuilder precompiledBundleCollectionBuilder;
        readonly CassetteSettings settings;

        public BundleCollectionBuilder(CassetteSettings settings, ProductionModeBundleCollectionBuilder productionModeBundleCollectionBuilder, DebugModeBundleCollectionBuilder debugModeBundleCollectionBuilder, PrecompiledBundleCollectionBuilder precompiledBundleCollectionBuilder)
        {
            this.settings = settings;
            this.productionModeBundleCollectionBuilder = productionModeBundleCollectionBuilder;
            this.debugModeBundleCollectionBuilder = debugModeBundleCollectionBuilder;
            this.precompiledBundleCollectionBuilder = precompiledBundleCollectionBuilder;
        }

        public void BuildBundleCollection(BundleCollection bundles)
        {
            if (settings.PrecompiledManifestFile.Exists)
            {
                precompiledBundleCollectionBuilder.BuildBundleCollection(bundles);
            }
            else if (settings.IsDebuggingEnabled)
            {
                debugModeBundleCollectionBuilder.BuildBundleCollection(bundles);
            }
            else
            {
                productionModeBundleCollectionBuilder.BuildBundleCollection(bundles);
            }
        }
    }
}