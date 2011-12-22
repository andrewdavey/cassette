using System.Collections.Generic;

namespace Cassette.Persistence
{
    interface IBundleCache
    {
        bool InitializeBundlesFromCacheIfUpToDate(IEnumerable<Bundle> unprocessedSourceBundles);
        void SaveBundleContainer(IBundleContainer bundleContainer);
        void Clear();
    }
}