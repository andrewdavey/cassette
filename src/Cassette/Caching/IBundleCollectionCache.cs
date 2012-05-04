namespace Cassette.Caching
{
    interface IBundleCollectionCache
    {
        CacheReadResult Read();
        void Write(BundleCollection bundles, string version);
        void Clear();
    }
}