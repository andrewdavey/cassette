using System.Linq;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly BundleCollection runtimeGeneratedBundles;
        readonly ICassetteManifestCache cache;
        readonly CassetteSettings settings;

        public CachedBundleContainerFactory(BundleCollection runtimeGeneratedBundles, ICassetteManifestCache cache, CassetteSettings settings)
            : base(settings)
        {
            this.runtimeGeneratedBundles = runtimeGeneratedBundles;
            this.cache = cache;
            this.settings = settings;
        }

        public override IBundleContainer CreateBundleContainer()
        {
            var currentManifest = CreateCassetteManifestFromRuntimeGeneratedBundles();
            var cachedManifest = cache.LoadCassetteManifest();

            var bundles = CreateBundles(cachedManifest, currentManifest);
            var externalBundles = CreateExternalBundlesUrlReferences(bundles);
            var allBundles = bundles.Concat(externalBundles);

            return new BundleContainer(allBundles);
        }

        BundleCollection CreateBundles(CassetteManifest cachedManifest, CassetteManifest currentManifest)
        {
            if (CanUseCachedBundles(cachedManifest, currentManifest))
            {
                Trace.Source.TraceInformation("Using cache.");
                return cachedManifest.CreateBundleCollection(settings);
            }
            else
            {
                return ProcessAndCacheAndGetRuntimeGeneratedBundles();
            }
        }

        bool CanUseCachedBundles(CassetteManifest cachedManifest, CassetteManifest currentManifest)
        {
            return cachedManifest.Equals(currentManifest) &&
                   cachedManifest.IsUpToDateWithFileSystem(settings.SourceDirectory);
        }

        BundleCollection ProcessAndCacheAndGetRuntimeGeneratedBundles()
        {
            ProcessAllBundles(runtimeGeneratedBundles);

            var manifest = CreateCassetteManifestFromRuntimeGeneratedBundles();

            Trace.Source.TraceInformation("Saving cache.");
            cache.SaveCassetteManifest(manifest);
            return manifest.CreateBundleCollection(settings);
        }

        CassetteManifest CreateCassetteManifestFromRuntimeGeneratedBundles()
        {
            var bundleManifests = runtimeGeneratedBundles.Select(bundle => bundle.CreateBundleManifest());
            return new CassetteManifest(settings.Version, bundleManifests);
        }
    }
}