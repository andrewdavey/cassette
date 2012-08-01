using System.Collections.Generic;

namespace Cassette
{
    public static class BundleEnumerableExtensions
    {
        public static void Accept(this IEnumerable<Bundle> bundles, IBundleVisitor bundleVisitor)
        {
            foreach (var bundle in bundles)
            {
                bundle.Accept(bundleVisitor);
            }
        }     
    }
}