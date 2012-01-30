using System.Collections.Generic;
using System.Linq;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly ICassetteManifestCache cache;
        readonly CassetteSettings settings;
        Bundle[] bundlesArray;

        public CachedBundleContainerFactory(ICassetteManifestCache cache, CassetteSettings settings)
            : base(settings)
        {
            this.cache = cache;
            this.settings = settings;
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles)
        {
            bundlesArray = unprocessedBundles.ToArray();
            
            var currentManifest = CreateCassetteManifest();
            var cachedManifest = cache.LoadCassetteManifest();

            bundlesArray = CreateBundles(cachedManifest, currentManifest);

            var externalBundles = CreateExternalBundlesUrlReferences(bundlesArray);
            var allBundles = bundlesArray.Concat(externalBundles);
            return new BundleContainer(allBundles);
        }

        Bundle[] CreateBundles(CassetteManifest cachedManifest, CassetteManifest currentManifest)
        {
            var canUseCachedBundles = cachedManifest.Equals(currentManifest)
                                   && cachedManifest.IsUpToDateWithFileSystem(settings.SourceDirectory);
            if (canUseCachedBundles)
            {
                UseCachedBundles(cachedManifest);
            }
            else
            {
                CacheAndUseCurrentBundles();
            }
            return bundlesArray;
        }

        void UseCachedBundles(CassetteManifest cachedManifest)
        {
            bundlesArray = cachedManifest.CreateBundles().ToArray();
            ProcessAllBundles(bundlesArray);
        }

        void CacheAndUseCurrentBundles()
        {
            ProcessAllBundles(bundlesArray);
            var fullManifest = CreateCassetteManifest();
            cache.SaveCassetteManifest(fullManifest);
            UseCachedBundles(fullManifest);
        }

        CassetteManifest CreateCassetteManifest()
        {
            var bundleManifests = bundlesArray.Select(bundle => bundle.CreateBundleManifest());
            return new CassetteManifest(bundleManifests);
        }
    }
}