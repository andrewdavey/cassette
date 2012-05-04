#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;
#endif

namespace Cassette
{
    class AssetDeserializer
    {
        XElement assetElement;
        string assetPath;

        public IAsset Deserialize(XElement element)
        {
            assetElement = element;
            assetPath = GetPathAttribute();
            var references = DeserializeReferences();
            var type = GetAssetCacheValidatorType();
            return new DeserializedAsset(assetPath, references, type);
        }

        Type GetAssetCacheValidatorType()
        {
            var typeName = assetElement.AttributeValueOrThrow(
                "AssetCacheValidatorType",
                () => new CassetteDeserializationException("Asset manifest element missing \"AssetCacheValidatorType\" attribute.")
            );
            return Type.GetType(typeName);
        }

        string GetPathAttribute()
        {
            return assetElement.AttributeValueOrThrow(
                "Path",
                () => new CassetteDeserializationException("Asset manifest element missing \"Path\" attribute.")
            );
        }

        IEnumerable<AssetReference> DeserializeReferences()
        {
            return assetElement.Elements("Reference").Select(DeserializeReference);
        }

        AssetReference DeserializeReference(XElement referenceElement)
        {
            var path = GetReferencePath(referenceElement);
            var sourceLineNumber = GetSourceLineNumber(referenceElement);
            var type = GetReferenceType(referenceElement);
            return new AssetReference(assetPath, path, sourceLineNumber, type);
        }

        string GetReferencePath(XElement referenceElement)
        {
            return referenceElement.AttributeValueOrThrow(
                "Path", 
                () => new CassetteDeserializationException("Reference manifest element missing \"Path\" attribute.")
            );
        }

        AssetReferenceType GetReferenceType(XElement referenceElement)
        {
            return ParseAssetReferenceType(
                referenceElement.AttributeValueOrThrow(
                    "Type",
                    () => new CassetteDeserializationException("Reference manifest element missing \"Type\" attribute.")
                )
            );
        }

        int GetSourceLineNumber(XElement referenceElement)
        {
            return ParseSourceLineNumber(
                referenceElement.AttributeValueOrThrow(
                    "SourceLineNumber",
                    () => new CassetteDeserializationException("Reference manifest element missing \"SourceLineNumber\" attribute.")
                )
            );
        }

        AssetReferenceType ParseAssetReferenceType(string typeString)
        {
            AssetReferenceType type;
#if NET35
            if (Enum<AssetReferenceType>.TryParse(typeString, out type))
            {
                return type;
            }
#else
            if (Enum.TryParse(typeString, out type)) {
                return type;
            }
#endif
            throw new CassetteDeserializationException(string.Format("Invalid asset reference type \"{0}\".", typeString));
        }

        int ParseSourceLineNumber(string sourceLineNumberString)
        {
            int sourceLineNumber;
            if (int.TryParse(sourceLineNumberString, out sourceLineNumber))
            {
                return sourceLineNumber;
            }
            throw new CassetteDeserializationException(string.Format("Invalid asset reference source line number \"{0}\".", sourceLineNumberString));
        }
    }
}