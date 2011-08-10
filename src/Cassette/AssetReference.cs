namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string referencedPath, IAsset sourceAsset, int sourceLineNumber, AssetReferenceType type)
        {
            ReferencedPath = referencedPath;
            SourceAsset = sourceAsset;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        /// <summary>
        /// Path to an asset or module.
        /// </summary>
        public string ReferencedPath { get; private set; }
        /// <summary>
        /// The asset that made this reference.
        /// </summary>
        public IAsset SourceAsset { get; private set; }
        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        public int SourceLineNumber { get; private set; }
        public AssetReferenceType Type { get; private set; }
    }
}
