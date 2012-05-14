namespace Cassette
{
    class RuntimeBundleCollectionInitializer : IBundleCollectionInitializer
    {
        readonly CassetteSettings settings;
        readonly CacheAwareBundleCollectionInitializer cacheAwareBundleCollectionInitializer;
        readonly BundleCollectionInitializer bundleCollectionInitializer;

        public RuntimeBundleCollectionInitializer(
            CassetteSettings settings,
            CacheAwareBundleCollectionInitializer cacheAwareBundleCollectionInitializer,
            BundleCollectionInitializer bundleCollectionInitializer)
        {
            this.settings = settings;
            this.cacheAwareBundleCollectionInitializer = cacheAwareBundleCollectionInitializer;
            this.bundleCollectionInitializer = bundleCollectionInitializer;
        }

        public void Initialize(BundleCollection bundles)
        {
            if (settings.IsDebuggingEnabled)
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