using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cassette.IO;

namespace Cassette.Caching
{
    class BundleCollectionCacheWriter
    {
        readonly IDirectory cacheDirectory;
        readonly string manifestFilename;

        public BundleCollectionCacheWriter(IDirectory cacheDirectory, string manifestFilename)
        {
            this.cacheDirectory = cacheDirectory;
            this.manifestFilename = manifestFilename;
        }

        public void WriteManifestFile(Manifest manifest)
        {
            var manifestDocuent = new XDocument(
                new XElement(
                    "BundleCollection",
                    new XAttribute("Version", manifest.Version),
                    new XAttribute("IsPrecompiled", manifest.IsPrecompiled)
                )
            );
            SerializeBundlesIntoManifest(manifest.Bundles, manifestDocuent);
            using (var manifestStream = OpenManifestFileForWriting())
            {
                manifestDocuent.Save(manifestStream);
                manifestStream.Flush();
            }
        }

        void SerializeBundlesIntoManifest(IEnumerable<Bundle> bundles, XDocument manifestDocuent)
        {
            foreach (var bundle in bundles)
            {
                bundle.SerializeInto(manifestDocuent.Root);
            }
        }

        Stream OpenManifestFileForWriting()
        {
            cacheDirectory.Create();
            var file = cacheDirectory.GetFile(manifestFilename);
            return file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        }

        public void WriteBundleContentFiles(IEnumerable<Bundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                WriteBundleContentFile(bundle);
            }
        }

        void WriteBundleContentFile(Bundle bundle)
        {
            var file = cacheDirectory.GetFile(bundle.CacheFilename);
            file.Directory.Create();
            using (var contentFileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bundleContent = bundle.OpenStream())
            {
                bundleContent.CopyTo(contentFileStream);
                contentFileStream.Flush();
            }
        }
    }
}