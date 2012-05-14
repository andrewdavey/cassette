using System;

namespace Cassette.Caching
{
    public interface IAssetCacheValidator
    {
        bool IsValid(string assetPath, DateTime asOfDateTime);
    }
}