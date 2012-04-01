using System.Collections.Generic;
using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette
{
    class BundleCollectionBuilder : IBundleCollectionBuilder
    {
        readonly CassetteSettings settings;
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        readonly ICassetteManifestCache manifestCache;
        readonly ExternalBundleGenerator externalBundleGenerator;
        readonly IUrlModifier urlModifier;

        public BundleCollectionBuilder(CassetteSettings settings, IEnumerable<IBundleDefinition> bundleDefinitions, ICassetteManifestCache manifestCache, ExternalBundleGenerator externalBundleGenerator, IUrlModifier urlModifier)
        {
            this.settings = settings;
            this.bundleDefinitions = bundleDefinitions;
            this.manifestCache = manifestCache;
            this.externalBundleGenerator = externalBundleGenerator;
            this.urlModifier = urlModifier;
        }

        public void BuildBundleCollection(BundleCollection bundles)
        {
            IBundleCollectionBuilder builder;
            if (settings.PrecompiledManifestFile.Exists)
            {
                builder = new PrecompiledBundleCollectionBuilder(settings.PrecompiledManifestFile, urlModifier);
            }
            else if (settings.IsDebuggingEnabled)
            {
                builder = new DebugModeBundleCollectionBuilder(bundleDefinitions, externalBundleGenerator);
            }
            else
            {
                builder = new ProductionModeBundleCollectionBuilder(bundleDefinitions, manifestCache, urlModifier, settings, externalBundleGenerator);
            }

            builder.BuildBundleCollection(bundles);
        }
    }
}