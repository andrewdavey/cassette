using System.Collections.Generic;
using System.Linq;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly BundleCollection runtimeGeneratedBundles;
        readonly ICassetteManifestCache cache;
        readonly CassetteSettings settings;
        readonly IUrlModifier urlModifier;

        public CachedBundleContainerFactory(BundleCollection runtimeGeneratedBundles, ICassetteManifestCache cache, CassetteSettings settings, IBundleFactoryProvider bundleFactoryProvider, IUrlModifier urlModifier)
            : base(settings, bundleFactoryProvider)
        {
            this.runtimeGeneratedBundles = runtimeGeneratedBundles;
            this.cache = cache;
            this.settings = settings;
            this.urlModifier = urlModifier;
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

        IEnumerable<Bundle> CreateBundles(CassetteManifest cachedManifest, CassetteManifest currentManifest)
        {
            if (CanUseCachedBundles(cachedManifest, currentManifest))
            {
                Trace.Source.TraceInformation("Using cache.");
                return cachedManifest.CreateBundles(urlModifier);
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

        IEnumerable<Bundle> ProcessAndCacheAndGetRuntimeGeneratedBundles()
        {
            ProcessAllBundles(runtimeGeneratedBundles);

            var manifest = CreateCassetteManifestFromRuntimeGeneratedBundles();

            Trace.Source.TraceInformation("Saving cache.");
            cache.SaveCassetteManifest(manifest);
            return manifest.CreateBundles(urlModifier);
        }

        CassetteManifest CreateCassetteManifestFromRuntimeGeneratedBundles()
        {
            var bundleManifests = runtimeGeneratedBundles.Select(bundle => bundle.CreateBundleManifest());
            return new CassetteManifest(settings.Version, bundleManifests);
        }
    }
}