using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Manifests
{
    class AssetManifestReader
    {
        readonly XElement element;

        public AssetManifestReader(XElement element)
        {
            this.element = element;
        }

        public AssetManifest Read()
        {
            var assetManifest = new AssetManifest
            {
                Path = GetPathAttribute()
            };
            AddRawFileReferences(assetManifest);
            return assetManifest;
        }

        string GetPathAttribute()
        {
            return element.AttributeValueOrThrow(
                "Path",
                () => new InvalidBundleManifestException("Asset manifest element missing \"Path\" attribute.")
            );
        }

        void AddRawFileReferences(AssetManifest assetManifest)
        {
            foreach (var rawFileReferenceElement in element.Elements("RawFileReference"))
            {
                AddRawFileReference(assetManifest, rawFileReferenceElement);
            }
        }

        void AddRawFileReference(AssetManifest assetManifest, XElement rawFileReferenceElement)
        {
            var path = rawFileReferenceElement.AttributeValueOrThrow("Path", () => new InvalidBundleManifestException("RawFileReference manifest element missing \"Path\" attribute."));
            assetManifest.RawFileReferences.Add(path);
        }
    }
}