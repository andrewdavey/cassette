using Cassette.TinyIoC;

namespace Cassette.HtmlAppCache
{
    [AutoRegisterImplementations]
    public interface ICacheManifestProvider
    {
        CacheManifest GetCacheManifest();
    }
}