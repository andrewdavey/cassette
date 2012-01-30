using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Cassette.Manifests
{
    class AssetManifestWriter
    {
        readonly XElement container;

        public AssetManifestWriter(XElement container)
        {
            this.container = container;
        }

        public void Write(AssetManifest assetManifest)
        {
            container.Add(AssetElement(assetManifest));
        }

        XElement AssetElement(AssetManifest assetManifest)
        {
            return new XElement(
                "Asset",
                new XAttribute("Path", assetManifest.Path),
                RawFileReferenceElements(assetManifest.RawFileReferences)
            );
        }

        IEnumerable<XElement> RawFileReferenceElements(IEnumerable<string> rawFileReferences)
        {
            return rawFileReferences.Select(
                path => new XElement(
                    "RawFileReference",
                    new XAttribute("Path", path)
                )
            );
        }
    }
}