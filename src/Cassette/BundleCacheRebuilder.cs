using Cassette.Caching;

namespace Cassette
{
    public interface IBundleCacheRebuilder
    {
        void RebuildCache();
    }

    class BundleCacheRebuilder : IBundleCacheRebuilder
    {
        readonly BundleCollection bundles;
        readonly IBundleCollectionCache cache;
        readonly IBundleCollectionInitializer bundleCollectionInitializer;

        BundleCacheRebuilder(BundleCollection bundles, IBundleCollectionCache cache, IBundleCollectionInitializer bundleCollectionInitializer)
        {
            this.bundles = bundles;
            this.cache = cache;
            this.bundleCollectionInitializer = bundleCollectionInitializer;
        }

        public void RebuildCache()
        {
            cache.Clear();
            using (bundles.GetWriteLock())
            {
                bundles.Clear();
                bundleCollectionInitializer.Initialize(bundles);
            }
        }
    }
}
