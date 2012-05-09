using System;
using Cassette.IO;

namespace Cassette.Caching
{
    class BundleCollectionCache : IBundleCollectionCache
    {
        const string ManifestFilename = "manifest.xml";

        readonly IDirectory cacheDirectory;
        readonly Func<string, IBundleDeserializer<Bundle>> getDeserializerForBundleTypeName;

        public BundleCollectionCache(IDirectory cacheDirectory, Func<string, IBundleDeserializer<Bundle>> getDeserializerForBundleTypeName)
        {
            this.cacheDirectory = cacheDirectory;
            this.getDeserializerForBundleTypeName = getDeserializerForBundleTypeName;
        }
        
        public CacheReadResult Read()
        {
            var reader = new BundleCollectionCacheReader(cacheDirectory, ManifestFilename, getDeserializerForBundleTypeName);
            return reader.Read();
        }

        public void Write(BundleCollection bundles, string version)
        {
            var writer = new BundleCollectionCacheWriter(cacheDirectory, ManifestFilename);
            writer.WriteManifestFile(bundles, version);
            writer.WriteBundleContentFiles(bundles);
        }

        public void Clear()
        {
            cacheDirectory.Delete();
            cacheDirectory.Create();
        }
    }
}