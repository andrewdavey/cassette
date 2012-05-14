namespace Cassette
{
    public interface IUrlGenerator
    {
        string CreateBundleUrl(Bundle bundle);
        string CreateAssetUrl(IAsset asset);
        string CreateRawFileUrl(string filename, string hash);
        /// <summary>
        /// Converts an application relative path into an absolute URL path. For example "~/images/test.png" becomes "/virtual-directory/images/test.png".
        /// </summary>
        string CreateAbsolutePathUrl(string applicationRelativePath);
    }
}