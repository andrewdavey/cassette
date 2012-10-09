using System;
using Cassette.IO;

namespace Cassette.Caching
{
    public class BundleCollectionCache : IBundleCollectionCache
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

        public void Write(Manifest manifest)
        {
            var writer = new BundleCollectionCacheWriter(cacheDirectory, ManifestFilename);
            writer.WriteManifestFile(manifest);
            writer.WriteBundleContentFiles(manifest.Bundles);
        }

        public void Clear()
        {
            cacheDirectory.Delete();
            cacheDirectory.Create();
        }
    }
}