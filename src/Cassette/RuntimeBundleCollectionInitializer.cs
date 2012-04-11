using Cassette.Configuration;

namespace Cassette
{
    class RuntimeBundleCollectionInitializer : IBundleCollectionInitializer
    {
        readonly CacheAwareBundleCollectionInitializer cacheAwareBundleCollectionInitializer;
        readonly BundleCollectionInitializer bundleCollectionInitializer;
        readonly PrecompiledBundleCollectionInitializer precompiledBundleCollectionInitializer;
        readonly CassetteSettings settings;

        public RuntimeBundleCollectionInitializer(CassetteSettings settings, CacheAwareBundleCollectionInitializer cacheAwareBundleCollectionInitializer, BundleCollectionInitializer bundleCollectionInitializer, PrecompiledBundleCollectionInitializer precompiledBundleCollectionInitializer)
        {
            this.settings = settings;
            this.cacheAwareBundleCollectionInitializer = cacheAwareBundleCollectionInitializer;
            this.bundleCollectionInitializer = bundleCollectionInitializer;
            this.precompiledBundleCollectionInitializer = precompiledBundleCollectionInitializer;
        }

        public void Initialize(BundleCollection bundles)
        {
            if (settings.PrecompiledManifestFile.Exists)
            {
                precompiledBundleCollectionInitializer.Initialize(bundles);
            }
            else if (settings.IsDebuggingEnabled)
            {
                bundleCollectionInitializer.Initialize(bundles);
            }
            else
            {
                cacheAwareBundleCollectionInitializer.Initialize(bundles);
            }
        }
    }
}