using System;
using Cassette.Utilities;

namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string fromAssetPath, string toPath, int sourceLineNumber, AssetReferenceType type)
        {
            ValidatePath(toPath, type);

            ToPath = toPath;
            FromAssetPath = fromAssetPath;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        static void ValidatePath(string path, AssetReferenceType type)
        {
            if (type == AssetReferenceType.Url)
            {
                if (!path.IsUrl())
                {
                    throw new ArgumentException(string.Format("Referenced path must be a URL.Path: {0}", path), "path");
                }
            }
            else
            {
                if (!path.StartsWith("~"))
                {
                    throw new ArgumentException(string.Format("Referenced path must be application relative and start with a \"~\". Path: {0}", path), "path");
                }
            }
        }

        /// <summary>
        /// The path of the asset that made this reference.
        /// </summary>
        public string FromAssetPath { get; private set; }

        /// <summary>
        /// Path to an asset, bundle or a URL.
        /// </summary>
        public string ToPath { get; private set; }

        /// <summary>
        /// The type of reference.
        /// </summary>
        public AssetReferenceType Type { get; private set; }

        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        public int SourceLineNumber { get; private set; }
    }
}
