using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

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
            return BundleUrls(bundle).Concat(FileUrls(bundle));
        }

        IEnumerable<string> BundleUrls(Bundle bundle)
        {
            return bundle.GetUrls(settings.IsDebuggingEnabled, urlGenerator);
        }

        IEnumerable<string> FileUrls(Bundle bundle)
        {
            var collector = new FileUrlCollector(urlGenerator);
            bundle.Accept(collector);
            return collector.Urls;
        }
    }
}