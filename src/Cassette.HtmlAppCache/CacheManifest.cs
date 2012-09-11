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
    }
}