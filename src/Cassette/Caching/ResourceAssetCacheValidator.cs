using System;
using System.IO;

namespace Cassette.Caching
{
    public class ResourceAssetCacheValidator : IAssetCacheValidator
    {
        public ResourceAssetCacheValidator(BundleCollection bundles)
        {
            this.bundles = bundles;
        }

        readonly BundleCollection bundles;

        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            using (bundles.GetReadLock())
            {
                Bundle bundle;
                IAsset asset;

				if (!bundles.TryGetAssetByPath(assetPath, out asset, out bundle))
					return false;

                var resourceAsset = asset as ResourceAsset;
                if (resourceAsset == null)
                    return true;

                var lastModified = File.GetLastWriteTimeUtc(resourceAsset.Assembly.Location);
                return lastModified <= asOfDateTime;
            }

        }
    }
}