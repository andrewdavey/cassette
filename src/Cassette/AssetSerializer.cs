using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Cassette.BundleProcessing;

namespace Cassette
{
    class AssetSerializer : IBundleVisitor
    {
        readonly XElement container;

        public AssetSerializer(XElement container)
        {
            this.container = container;
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            if (asset is ConcatenatedAsset) return;

            Serialize(asset);
        }

        public void Serialize(IAsset asset)
        {
            container.Add(AssetElement(asset));
        }

        XElement AssetElement(IAsset asset)
        {
            return new XElement(
                "Asset",
                new XAttribute("Path", asset.Path),
                new XAttribute("AssetCacheValidatorType", asset.AssetCacheValidatorType.AssemblyQualifiedName),
                ReferenceElements(asset.References)
            );
        }

        IEnumerable<XElement> ReferenceElements(IEnumerable<AssetReference> references)
        {
            return references.Select(ReferenceElement);
        }

        XElement ReferenceElement(AssetReference reference)
        {
            return new XElement(
                "Reference",
                new XAttribute("Path", reference.ToPath),
                new XAttribute("Type", Enum.GetName(typeof(AssetReferenceType), reference.Type)),
                new XAttribute("SourceLineNumber", reference.SourceLineNumber.ToString(CultureInfo.InvariantCulture))
            );
        }
    }
}