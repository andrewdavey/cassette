using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    static class BundleContainerExtensions
    {
        public static bool TryGetAssetByPath(this IEnumerable<Bundle> bundles, string path, out IAsset asset, out Bundle bundle)
        {
            var results =
                from b in bundles
                where b.ContainsPath(path)
                let a = b.FindAssetByPath(path)
                where a != null
                select new { Bundle = b, Asset = a };

            var result = results.FirstOrDefault();
            if (result != null)
            {
                asset = result.Asset;
                bundle = result.Bundle;
                return true;
            }
            else
            {
                asset = null;
                bundle = null;
                return false;
            }
        }
    }
}
