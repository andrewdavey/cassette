namespace Cassette.Caching
{
    interface IBundleCollectionCache
    {
        CacheReadResult Read();
        void Write(BundleCollection bundles);
        void Clear();
    }
}