namespace Cassette
{
    public interface IUrlGenerator
    {
        string CreateAssetUrl(Module module, IAsset asset);
        string CreateAssetCompileUrl(Module module, IAsset asset);
        string CreateImageUrl(string filename, string hash);
    }
}
