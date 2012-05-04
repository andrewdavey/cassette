using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Caching
{
    class BundleCollectionCache : IBundleCollectionCache
    {
        const string ManifestFilename = "manifest.xml";

        static readonly Dictionary<string, string> FileExtensionsByContentType = new Dictionary<string, string>
        {
            { "text/javascript", ".js" },
            { "text/css", ".css" },
            { "text/html", ".htm" }
        };

        readonly IDirectory cacheDirectory;
        readonly Func<string, IBundleDeserializer<Bundle>> getDeserializerForBundleTypeName;

        public BundleCollectionCache(IDirectory cacheDirectory, Func<string, IBundleDeserializer<Bundle>> getDeserializerForBundleTypeName)
        {
            this.cacheDirectory = cacheDirectory;
            this.getDeserializerForBundleTypeName = getDeserializerForBundleTypeName;
        }

        public CacheReadResult Read()
        {
            var file = cacheDirectory.GetFile(ManifestFilename);
            if (file.Exists)
            {
                return CacheReadResult.Success(DeserializeBundles(file), file.LastWriteTimeUtc);
            }
            else
            {
                return CacheReadResult.Failed();
            }
        }

        IEnumerable<Bundle> DeserializeBundles(IFile manifestFile)
        {
            using (var manifestStream = manifestFile.OpenRead())
            {
                var manifestDocument = XDocument.Load(manifestStream);
                var bundleElements = manifestDocument.Root.Elements();
                return CreateBundles(bundleElements).ToArray();
            }
        }

        IEnumerable<Bundle> CreateBundles(IEnumerable<XElement> bundleElements)
        {
            return from bundleElement in bundleElements
                   let deserializer = GetDeserializer(bundleElement)
                   select deserializer.Deserialize(bundleElement, cacheDirectory);
        }

        IBundleDeserializer<Bundle> GetDeserializer(XElement bundleElement)
        {
            var bundleTypeName = bundleElement.Name.LocalName;
            return getDeserializerForBundleTypeName(bundleTypeName);
        }

        public void Write(BundleCollection bundles, string version)
        {
            WriteManifestFile(bundles, version);

            foreach (var bundle in bundles)
            {
                var filename = bundle.Hash.ToHexString() + FileExtensionsByContentType[bundle.ContentType];

                using (var bundleContent = bundle.OpenStream())
                using (var contentFileStream = cacheDirectory.GetFile(filename).Open(FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    bundleContent.CopyTo(contentFileStream);
                    contentFileStream.Flush();
                }
            }
        }

        void WriteManifestFile(IEnumerable<Bundle> bundles, string version)
        {
            var manifestDocuent = new XDocument(
                new XElement("BundleCollection", new XAttribute("Version", version))
            );
            SerializeBundlesIntoManifest(bundles, manifestDocuent);
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
            var file = cacheDirectory.GetFile(ManifestFilename);
            return file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        }

        public void Clear()
        {
            var files = cacheDirectory.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                file.Delete();
            }
        }
    }
}