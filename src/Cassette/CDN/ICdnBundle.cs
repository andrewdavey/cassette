using Cassette.IO;

namespace Cassette.CDN
{
    public interface ICdnBundle
    {
        string CdnRoot { get; set; }
        string CdnCacheRoot { get; set; }
    }
}
