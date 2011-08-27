using System;
using System.Xml.Linq;

namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string path, IAsset sourceAsset, int sourceLineNumber, AssetReferenceType type)
        {
            if (type != AssetReferenceType.Url && path.StartsWith("~") == false)
            {
                throw new ArgumentException("Referenced path must be application relative and start with a \"~\".");
            }
            Path = path;
            SourceAsset = sourceAsset;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        /// <summary>
        /// Path to an asset or module.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The type of reference.
        /// </summary>
        public AssetReferenceType Type { get; set; }

        /// <summary>
        /// The asset that made this reference.
        /// </summary>
        public IAsset SourceAsset { get; private set; }

        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        public int SourceLineNumber { get; private set; }

        public XElement CreateCacheManifest()
        {
            return new XElement("Reference",
                new XAttribute("Type", Enum.GetName(typeof(AssetReferenceType), Type)),
                new XAttribute("Path", Path),
                new XAttribute("SourceLineNumber", SourceLineNumber)
            );
        }
    }
}