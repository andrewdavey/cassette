namespace Cassette
{
    public class AssetReference
    {
        public AssetReference(string filename, AssetReferenceType type)
        {
            Filename = filename;
            Type = type;
        }

        public string Filename { get; private set; }
        public AssetReferenceType Type { get; private set; }
    }
}
