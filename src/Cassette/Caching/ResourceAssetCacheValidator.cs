using System;

namespace Cassette.Caching
{
    public class ResourceAssetCacheValidator : IAssetCacheValidator
    {
        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            return true;
        }
    }
}