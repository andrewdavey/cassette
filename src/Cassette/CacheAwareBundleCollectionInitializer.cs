using System.Collections.Generic;
using Cassette.Caching;

namespace Cassette
{
    class CacheAwareBundleCollectionInitializer
    {
        readonly IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations;
        readonly IBundleCollectionCache cache;
        readonly ExternalBundleGenerator externalBundleGenerator;
        readonly BundleCollectionCacheValidator cacheValidator;
        BundleCollection bundles;
        CacheReadResult cacheReadResult;

        public CacheAwareBundleCollectionInitializer(IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations, IBundleCollectionCache cache, ExternalBundleGenerator externalBundleGenerator, BundleCollectionCacheValidator cacheValidator)
        {
            this.bundleConfigurations = bundleConfigurations;
            this.cache = cache;
            this.externalBundleGenerator = externalBundleGenerator;
            this.cacheValidator = cacheValidator;
        }

        public void Initialize(BundleCollection bundleCollection)
        {
            bundles = bundleCollection;
            using (bundles.GetWriteLock())
            {
                ClearBundles();
                AddBundlesFromConfigurations();
                ReadCache();
                if (IsCacheValid())
                {
                    UseCachedBundles();
                }
                else
                {
                    ProcessBundles();
                    WriteToCache();
                }

                bundles.BuildReferences();
            }
        }

        void ClearBundles()
        {
            bundles.Clear();
        }

        void AddBundlesFromConfigurations()
        {
            bundleConfigurations.Configure(bundles);
        }

        void ReadCache()
        {
            cacheReadResult = cache.Read();
        }

        bool IsCacheValid()
        {
            return cacheReadResult.IsSuccess && 
                   bundles.Equals(cacheReadResult.Bundles) && 
                   cacheValidator.IsValid(cacheReadResult);
        }

        void WriteToCache()
        {
            cache.Write(bundles);
        }

        void UseCachedBundles()
        {
            ClearBundles();
            bundles.AddRange(cacheReadResult.Bundles);
        }

        void ProcessBundles()
        {
            bundles.Process();
            AddBundlesForUrlReferences();
        }

        void AddBundlesForUrlReferences()
        {
            externalBundleGenerator.AddBundlesForUrlReferences(bundles);
        }
    }
}