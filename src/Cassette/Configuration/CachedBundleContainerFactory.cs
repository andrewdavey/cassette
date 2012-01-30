using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Persistence;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly IBundleCache cache;

        public CachedBundleContainerFactory(IBundleCache cache, IDictionary<Type, IBundleFactory<Bundle>> bundleFactories)
            : base(bundleFactories)
        {
            this.cache = cache;
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, CassetteSettings settings)
        {
            throw new NotImplementedException();
            /*
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();
            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray, settings);
            var allBundles = bundlesArray.Concat(externalBundles).ToArray();

            var currentManifest = new CassetteManifest(allBundles.Select(b => b.CreateBundleManifest()));
            var cachedManifest = new CassetteManifest();

            if (currentManifest.Equals(cachedManifest) && cachedManifest.IsSatisfiedBy(settings.SourceDirectory))
            {
                var cachedBundles = cachedManifest.CreateBundles();
                ProcessAllBundles(cachedBundles, settings);
                return new BundleContainer(cachedBundles);
            }
            else
            {
                ProcessAllBundles(allBundles, settings);
                var fullManifest = new CassetteManifest(allBundles.Select(b => b.CreateBundleManifest()));
                var writer = new CassetteManifestWriter(manifestFileStream);
                writer.Write(fullManifest);
                // TODO: write each bundle content file to cache dir.

                return new BundleContainer(allBundles);
            }
            */
        }
    }
}
