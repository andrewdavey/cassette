using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Persistence;

namespace Cassette.Configuration
{
    public class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly IBundleCache cache;

        public CachedBundleContainerFactory(IBundleCache cache, IDictionary<Type, IBundleFactory<Bundle>> bundleFactories)
            : base(bundleFactories)
        {
            this.cache = cache;
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles, ICassetteApplication application)
        {
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();
            InitializeAllBundles(bundlesArray, application);

            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray);

            if (cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray))
            {
                return new BundleContainer(bundlesArray.Concat(externalBundles));
            }
            else
            {
                ProcessAllBundles(bundlesArray, application);
                var container = new BundleContainer(bundlesArray.Concat(externalBundles));
                cache.SaveBundleContainer(container);
                cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray);
                return container;
            }
        }
    }
}