namespace Cassette
{
    /// <summary>
    /// A visitor that traverses a bundle and its assets.
    /// </summary>
    public interface IBundleVisitor
    {
        void Visit(Bundle bundle);
        void Visit(IAsset asset);
    }
}