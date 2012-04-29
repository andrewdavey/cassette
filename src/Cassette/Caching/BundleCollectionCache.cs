using Cassette.IO;

namespace Cassette.Caching
{
    class BundleCollectionCache : IBundleCollectionCache
    {
        readonly IDirectory cacheDirectory;
        readonly string manifestFilename = "bundles.xml";

        public BundleCollectionCache(IDirectory cacheDirectory)
        {
            this.cacheDirectory = cacheDirectory;
        }

        public CacheReadResult Read()
        {
            var manifestFile = cacheDirectory.GetFile(manifestFilename);

            return CacheReadResult.Failed();
        }

        public void Write(BundleCollection bundles)
        {
            
        }

        public void Clear()
        {
            
        }
    }
}