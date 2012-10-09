using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Caching;
using Trace = Cassette.Diagnostics.Trace;

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
                        Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer using static cache");
                        UseCachedBundles();
                    }
                    else
                    {
                        AddBundlesFromConfigurations();
                        if (IsCacheValid)
                        {
                            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer using runtime cache");
                            UseCachedBundles();
                        }
                        else
                        {
                            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer runtime cache is invalid");
                            ProcessBundles();
                            WriteToCache();
                        }
                    }
                }
                else
                {
                    Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer failed to read from cache");
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
            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer.ClearBundles");
            bundles.Clear();
        }

        void AddBundlesFromConfigurations()
        {
            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer.AddBundlesFromConfigurations");
            bundleConfigurations.Configure(bundles);
        }

        bool ReadCache()
        {
            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer.ReadCache");
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
                return settings.Version == cacheReadResult.Manifest.Version &&
                       bundles.Equals(cacheReadResult.Manifest.Bundles) &&
                       manifestValidator.IsValid(cacheReadResult.Manifest);
            }
        }

        void WriteToCache()
        {
            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer.WriteToCache");
            cache.Write(new Manifest(bundles, settings.Version));
        }

        void UseCachedBundles()
        {
            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer.UseCacheBundles");
            ClearBundles();
            bundles.AddRange(cacheReadResult.Manifest.Bundles);
        }

        void ProcessBundles()
        {
            Trace.Source.TraceInformation("CacheAwareBundleCollectionInitializer.ProcessBundles");
            bundles.Process();
        }

        void AddBundlesForUrlReferences()
        {
            externalBundleGenerator.AddBundlesForUrlReferences(bundles);
        }
    }
}