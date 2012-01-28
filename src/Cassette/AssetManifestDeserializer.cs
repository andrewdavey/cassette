using System.Xml.Linq;

namespace Cassette
{
    class AssetManifestDeserializer
    {
        public AssetManifest Deserialize(XElement assetElement)
        {
            var assetManifest = new AssetManifest
            {
                Path = assetElement.AttributeOrThrow("Path", () => new InvalidBundleManifestException("Asset manifest element missing \"Path\" attribute."))
            };
            AddRawFileReferences(assetElement, assetManifest);
            return assetManifest;
        }

        void AddRawFileReferences(XElement assetElement, AssetManifest assetManifest)
        {
            foreach (var element in assetElement.Elements("RawFileReference"))
            {
                AddRawFileReference(assetManifest, element);
            }
        }

        static void AddRawFileReference(AssetManifest assetManifest, XElement element)
        {
            var path = element.AttributeOrThrow("Path", () => new InvalidBundleManifestException("RawFileReference manifest element missing \"Path\" attribute."));
            assetManifest.RawFileReferences.Add(path);
        }
    }
}