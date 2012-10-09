namespace Cassette.Caching
{
    public interface IBundleCollectionCache
    {
        CacheReadResult Read();
        void Write(Manifest manifest);
        void Clear();
    }
}