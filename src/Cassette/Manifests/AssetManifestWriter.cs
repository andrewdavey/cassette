using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System;

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
                ReferenceElements(assetManifest.References)
            );
        }

        IEnumerable<XElement> ReferenceElements(IEnumerable<AssetReferenceManifest> rawFileReferences)
        {
            return rawFileReferences.Select(ReferenceElement);
        }

        XElement ReferenceElement(AssetReferenceManifest reference)
        {
            return new XElement(
                "Reference",
                new XAttribute("Path", reference.Path),
                new XAttribute("Type", Enum.GetName(typeof(AssetReferenceType), reference.Type)),
                new XAttribute("SourceLineNumber", reference.SourceLineNumber.ToString(CultureInfo.InvariantCulture))
            );
        }
    }
}