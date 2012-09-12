using System.Collections.Generic;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    public class CacheManifestBuilder
    {
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public CacheManifestBuilder(IUrlGenerator urlGenerator, CassetteSettings settings)
        {
            this.urlGenerator = urlGenerator;
            this.settings = settings;
        }

        public CacheManifest BuildCacheManifest(IEnumerable<Bundle> bundles)
        {
            var cacheUrls = bundles.SelectMany(GetUrls);
            var cacheManifest = new CacheManifest();
            foreach (var url in cacheUrls)
            {
                cacheManifest.Cache.Add(url);
            }
            return cacheManifest;
        }

        IEnumerable<string> GetUrls(Bundle bundle)
        {
            return bundle.GetUrls(settings.IsDebuggingEnabled, urlGenerator);
        }
    }
}