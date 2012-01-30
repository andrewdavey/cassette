using System;
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
            AddReferences(assetManifest);
            return assetManifest;
        }

        string GetPathAttribute()
        {
            return element.AttributeValueOrThrow(
                "Path",
                () => new InvalidCassetteManifestException("Asset manifest element missing \"Path\" attribute.")
            );
        }

        void AddReferences(AssetManifest assetManifest)
        {
            foreach (var referenceElement in element.Elements("Reference"))
            {
                AddReference(assetManifest, referenceElement);
            }
        }

        void AddReference(AssetManifest assetManifest, XElement referenceElement)
        {
            var path = referenceElement.AttributeValueOrThrow("Path", () => new InvalidCassetteManifestException("Reference manifest element missing \"Path\" attribute."));
            var typeString = referenceElement.AttributeValueOrThrow("Type", () => new InvalidCassetteManifestException("Reference manifest element missing \"Type\" attribute."));
            var sourceLineNumberString = referenceElement.AttributeValueOrThrow("SourceLineNumber", () => new InvalidCassetteManifestException("Reference manifest element missing \"SourceLineNumber\" attribute."));
            assetManifest.References.Add(new AssetReferenceManifest
            {
                Path = path,
                Type = ParseAssetReferenceType(typeString),
                SourceLineNumber = ParseSourceLineNumber(sourceLineNumberString)
            });
        }

        AssetReferenceType ParseAssetReferenceType(string typeString)
        {
            AssetReferenceType type;
            if (Enum.TryParse(typeString, out type))
            {
                return type;
            }
            throw new InvalidCassetteManifestException(string.Format("Invalid asset reference type \"{0}\".", typeString));
        }

        int ParseSourceLineNumber(string sourceLineNumberString)
        {
            int sourceLineNumber;
            if (int.TryParse(sourceLineNumberString, out sourceLineNumber))
            {
                return sourceLineNumber;
            }
            throw new InvalidCassetteManifestException(string.Format("Invalid asset reference source line number \"{0}\".", sourceLineNumberString));
        }
    }
}