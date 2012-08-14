using System;
using Cassette.Utilities;

namespace Cassette
{
    [ProtoBuf.ProtoContract]
    public class AssetReference
    {
        public AssetReference(string path, IAsset sourceAsset, int sourceLineNumber, AssetReferenceType type)
        {
            ValidatePath(path, type);

            Path = path;
            SourceAsset = sourceAsset;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        static void ValidatePath(string path, AssetReferenceType type)
        {
            if (type == AssetReferenceType.Url)
            {
                if (!path.IsUrl())
                {
                    throw new ArgumentException("Referenced path must be a URL.", "path");
                }
            }
            else
            {
                if (!path.StartsWith("~"))
                {
                    throw new ArgumentException(
                        "Referenced path must be application relative and start with a \"~\".",
                        "path"
                    );
                }
            }
        }

        /// <summary>
        /// Path to an asset or bundle.
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public string Path { get; private set; }

        /// <summary>
        /// The type of reference.
        /// </summary>
        [ProtoBuf.ProtoMember(2)]
        public AssetReferenceType Type { get; private set; }

        /// <summary>
        /// The asset that made this reference.
        /// </summary>
        public IAsset SourceAsset { get; private set; }

        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        [ProtoBuf.ProtoMember(3)]
        public int SourceLineNumber { get; private set; }
    }
}
