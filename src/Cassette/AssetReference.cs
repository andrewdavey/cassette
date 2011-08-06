namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string filename, int lineNumber, AssetReferenceType type)
        {
            Filename = filename;
            LineNumber = lineNumber;
            Type = type;
        }

        public string Filename { get; private set; }
        public int LineNumber { get; private set; }
        public AssetReferenceType Type { get; private set; }
    }
}
