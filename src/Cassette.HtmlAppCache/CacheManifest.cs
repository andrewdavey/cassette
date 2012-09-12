using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    public class CacheManifest
    {
        public CacheManifest()
        {
            Cache = new CacheSection();
            Network = new NetworkSection();
            Fallback = new FallbackSection();
        }

        public DateTime LastModified { get; set; }
        public CacheSection Cache { get; private set; }
        public NetworkSection Network { get; private set; }
        public FallbackSection Fallback { get; private set; }

        public override string ToString()
        {
            var sections = SectionsToStrings();
            return "CACHE MANIFEST\r\n" + string.Join("\r\n", sections);
        }

        IEnumerable<string> SectionsToStrings()
        {
            return from section in new object[] { Cache, Network, Fallback }
                   let sectionString = section.ToString()
                   where !string.IsNullOrEmpty(sectionString)
                   select sectionString;
        }

        public static CacheManifest Merge(CacheManifest manifest1, CacheManifest manifest2)
        {
            return new CacheManifest
            {
                Cache = CacheSection.Merge(manifest1.Cache, manifest2.Cache),
                Fallback = FallbackSection.Merge(manifest1.Fallback, manifest2.Fallback),
                Network = NetworkSection.Merge(manifest1.Network, manifest2.Network),
                LastModified = MaxLastModified(manifest1, manifest2)
            };
        }

        static DateTime MaxLastModified(CacheManifest manifest1, CacheManifest manifest2)
        {
            return manifest1.LastModified > manifest2.LastModified ? manifest1.LastModified : manifest2.LastModified;
        }
    }
}