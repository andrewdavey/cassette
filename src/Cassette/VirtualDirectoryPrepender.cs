namespace Cassette
{
    /// <summary>
    /// Prepends the virtual directory to the beginning of relative URLs.
    /// </summary>
    public class VirtualDirectoryPrepender : IUrlModifier
    {
        readonly string virtualDirectory;

        public VirtualDirectoryPrepender(string virtualDirectory)
        {
            this.virtualDirectory = virtualDirectory.TrimEnd('/');
        }

        public string Modify(string url)
        {
            return virtualDirectory + "/" + url.TrimStart('/');
        }
    }
}