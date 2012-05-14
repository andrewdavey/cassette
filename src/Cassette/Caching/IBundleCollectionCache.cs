namespace Cassette.Caching
{
    interface IBundleCollectionCache
    {
        CacheReadResult Read();
        void Write(Manifest manifest);
        void Clear();
    }
}