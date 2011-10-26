using Cassette.Utilities;

namespace Cassette
{
    class AssetFinder : IAssetVisitor
    {
        readonly string pathToFind;

        public AssetFinder(string pathToFind)
        {
            this.pathToFind = pathToFind;
        }

        public IAsset FoundAsset { get; private set; }

        public void Visit(Bundle bundle)
        {
            if (FoundAsset != null) return;
        }

        public void Visit(IAsset asset)
        {
            if (FoundAsset != null) return;

            if (PathUtilities.PathsEqual(asset.SourceFile.FullPath, pathToFind))
            {
                FoundAsset = asset;
            }
        }
    }
}