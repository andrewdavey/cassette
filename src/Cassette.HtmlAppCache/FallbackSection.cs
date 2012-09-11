using System.Collections.ObjectModel;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    public class FallbackSection : Collection<FallbackMapping>
    {
        public void Add(string urlNamespace, string fallbackUrl)
        {
            Add(new FallbackMapping(urlNamespace, fallbackUrl));
        }

        public override string ToString()
        {
            if (Count == 0) return "";
            return "FALLBACK:\r\n" + string.Join("\r\n", this.Select(mapping => mapping.ToString()));
        }
    }
}