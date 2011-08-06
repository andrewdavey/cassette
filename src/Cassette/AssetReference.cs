namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string referencedFilename, IAsset sourceAsset, int sourceLineNumber, AssetReferenceType type)
        {
            ReferencedFilename = referencedFilename;
            SourceAsset = sourceAsset;
            SourceLineNumber = sourceLineNumber;
            Type = type;
        }

        public string ReferencedFilename { get; private set; }
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
