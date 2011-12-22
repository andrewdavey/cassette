namespace Cassette
{
    public interface IUrlGenerator
    {
        string CreateBundleUrl(Bundle bundle);
        string CreateAssetUrl(IAsset asset);
        string CreateRawFileUrl(string filename, string hash);
    }
}