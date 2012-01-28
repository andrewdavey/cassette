using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cassette
{
    class BundleManifestDeserializer
    {
        public IEnumerable<BundleManifest> Deserialize(Stream xmlStream)
        {
            var document = XDocument.Load(xmlStream);
            if (document.Root == null)
            {
                throw new InvalidBundleManifestException("Bundles manifest XML missing root element.");
            }
            var manifestElements = document.Root.Elements();
            return manifestElements.Select(DeserializeBundleManifest);
        }

        BundleManifest DeserializeBundleManifest(XElement manifestElement)
        {
            var manifest = new BundleManifest
            {
                Path = manifestElement.AttributeOrThrow("Path", () => new InvalidBundleManifestException("Bundle manifest element missing \"Path\" attribute.")),
                ContentType = manifestElement.AttributeValueOrNull("ContentType"),
                PageLocation = manifestElement.AttributeValueOrNull("PageLocation")
            };
            AddAssets(manifestElement, manifest);
            AddReferences(manifestElement, manifest);
            return manifest;
        }

        void AddAssets(XElement manifestElement, BundleManifest manifest)
        {
            var assetElements = manifestElement.Elements("Asset");
            foreach (var assetElement in assetElements)
            {
                manifest.Assets.Add(DeserializeAssetManifest(assetElement));
            }
        }

        AssetManifest DeserializeAssetManifest(XElement assetElement)
        {
            var deserializer = new AssetManifestDeserializer();
            return deserializer.Deserialize(assetElement);
        }

        void AddReferences(XElement manifestElement, BundleManifest manifest)
        {
            var referenceElements = manifestElement.Elements("Reference");
            foreach (var referenceElement in referenceElements)
            {
                AddReference(manifest, referenceElement);
            }
        }

        void AddReference(BundleManifest manifest, XElement referenceElement)
        {
            var path = referenceElement.AttributeOrThrow("Path", () => new InvalidBundleManifestException("Reference manifest element missing \"Path\" attribute."));
            manifest.References.Add(path);
        }
    }
}