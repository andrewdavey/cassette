namespace Cassette
{
    public interface IAssetVisitor
    {
        void Visit(Module module);
        void Visit(IAsset asset);
    }
}
