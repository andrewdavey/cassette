namespace Cassette
{
    /// <summary>
    /// Transforms asset content.
    /// </summary>
    public interface IAssetTransformer
    {
        string Transform(string assetContent, IAsset asset);
    }
}