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
            using (bundleCollection.GetWriteLock())
            {
                bundles = bundleCollection;
                ClearBundles();
                if (ReadCache())
                {
                    if (IsStaticCache)
                    {
                        UseCachedBundles();
                    }
                    else
                    {
                        AddBundlesFromConfigurations();
                        if (IsCacheValid)
                        {
                            UseCachedBundles();
                        }
                        else
                        {
                            ProcessBundles();
                            WriteToCache();
                        }
                    }
                }
                else
                {
                    AddBundlesFromConfigurations();
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

        bool ReadCache()
        {
            cacheReadResult = cache.Read();
            return cacheReadResult.IsSuccess;
        }

        bool IsStaticCache
        {
            get { return cacheReadResult.Manifest.IsStatic; }
        }

        bool IsCacheValid
        {
            get
            {
                return bundles.Equals(cacheReadResult.Manifest.Bundles) &&
                       manifestValidator.IsValid(cacheReadResult.Manifest);
            }
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