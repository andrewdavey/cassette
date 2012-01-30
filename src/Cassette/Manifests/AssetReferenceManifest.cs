namespace Cassette.Manifests
{
    class AssetReferenceManifest
    {
        public AssetReferenceType Type { get; set; }
        public string Path { get; set; }
        public int SourceLineNumber { get; set; }
    }
}