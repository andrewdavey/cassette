using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Persistence;

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
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();

            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray, settings);

            if (cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray))
            {
                return new BundleContainer(bundlesArray.Concat(externalBundles));
            }
            else
            {
                ProcessAllBundles(bundlesArray, settings);
                var container = new BundleContainer(bundlesArray.Concat(externalBundles));
                cache.SaveBundleContainer(container);
                cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray);
                return container;
            }
        }
    }
}
