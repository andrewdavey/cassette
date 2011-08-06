namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string referencedFilename, int referencingLineNumber, AssetReferenceType type)
        {
            ReferencedFilename = referencedFilename;
            ReferencingLineNumber = referencingLineNumber;
            Type = type;
        }

        public string ReferencedFilename { get; private set; }
        /// <summary>
        /// The line number in the asset file that made this reference.
        /// </summary>
        public int ReferencingLineNumber { get; private set; }
        public AssetReferenceType Type { get; private set; }
    }
}
