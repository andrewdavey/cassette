using Cassette.Caching;

namespace Cassette
{
    class PrecompiledBundleCollectionInitializer
    {
        readonly IBundleCollectionCache cache;

        public PrecompiledBundleCollectionInitializer(IBundleCollectionCache cache)
        {
            this.cache = cache;
        }

        public void Initialize(BundleCollection bundles)
        {
            using (bundles.GetWriteLock())
            {
                var cacheReadResult = cache.Read();
                bundles.Clear();
                bundles.AddRange(cacheReadResult.Manifest.Bundles);
                bundles.BuildReferences();
            }
        }
    }
}