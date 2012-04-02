using Cassette.Configuration;

namespace Cassette
{
    class BundleCollectionBuilder : IBundleCollectionBuilder
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
            IBundleCollectionBuilder builder;
            if (settings.PrecompiledManifestFile.Exists)
            {
                builder = precompiledBundleCollectionBuilder;
            }
            else if (settings.IsDebuggingEnabled)
            {
                builder = debugModeBundleCollectionBuilder;
            }
            else
            {
                builder = productionModeBundleCollectionBuilder;
            }

            builder.BuildBundleCollection(bundles);
        }
    }
}