using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cassette.HtmlAppCache
{
    public class FallbackSection : Collection<FallbackMapping>
    {
        public FallbackSection()
        {
        }

        FallbackSection(IEnumerable<FallbackMapping> fallbackMappings)
        {
            foreach (var fallbackMapping in fallbackMappings)
            {
                Add(fallbackMapping);
            }
        }

        public void Add(string urlNamespace, string fallbackUrl)
        {
            Add(new FallbackMapping(urlNamespace, fallbackUrl));
        }

        public override string ToString()
        {
            if (Count == 0) return "";
            return "FALLBACK:\r\n" + string.Join("\r\n", this.Select(mapping => mapping.ToString()));
        }

        public static FallbackSection Merge(FallbackSection section1, FallbackSection section2)
        {
            var mappings = section1.Concat(section2).Distinct();
            return new FallbackSection(mappings);
        }
    }
}