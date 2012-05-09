using System.Collections.Generic;
using Cassette.Caching;

namespace Cassette
{
    class CacheAwareBundleCollectionInitializer
    {
        readonly IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations;
        readonly IBundleCollectionCache cache;
        readonly ExternalBundleGenerator externalBundleGenerator;
        readonly ManifestValidator manifestValidator;
        readonly CassetteSettings settings;
        BundleCollection bundles;
        CacheReadResult cacheReadResult;

        public CacheAwareBundleCollectionInitializer(IEnumerable<IConfiguration<BundleCollection>> bundleConfigurations, IBundleCollectionCache cache, ExternalBundleGenerator externalBundleGenerator, ManifestValidator manifestValidator, CassetteSettings settings)
        {
            this.bundleConfigurations = bundleConfigurations;
            this.cache = cache;
            this.externalBundleGenerator = externalBundleGenerator;
            this.manifestValidator = manifestValidator;
            this.settings = settings;
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

                AddBundlesForUrlReferences();
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
                   bundles.Equals(cacheReadResult.Manifest.Bundles) && 
                   manifestValidator.IsValid(cacheReadResult.Manifest);
        }

        void WriteToCache()
        {
            cache.Write(new Manifest(bundles, settings.Version));
        }

        void UseCachedBundles()
        {
            ClearBundles();
            bundles.AddRange(cacheReadResult.Manifest.Bundles);
        }

        void ProcessBundles()
        {
            bundles.Process();
        }

        void AddBundlesForUrlReferences()
        {
            externalBundleGenerator.AddBundlesForUrlReferences(bundles);
        }
    }
}