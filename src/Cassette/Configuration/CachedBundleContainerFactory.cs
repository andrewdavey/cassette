using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Persistence;

namespace Cassette.Configuration
{
    public class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly IBundleCache cache;

        public CachedBundleContainerFactory(IBundleCache cache, ICassetteApplication application, IDictionary<Type, IBundleFactory<Bundle>> bundleFactories)
            : base(application, bundleFactories)
        {
            this.cache = cache;
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles)
        {
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();
            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray);

            if (cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray))
            {
                return new BundleContainer(bundlesArray.Concat(externalBundles));
            }
            else
            {
                ProcessAllBundles(bundlesArray);
                var container = new BundleContainer(bundlesArray.Concat(externalBundles));
                cache.SaveBundleContainer(container);
                cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray);
                return container;
            }
        }
    }
}