using System;
using Cassette.Caching;

namespace Cassette.Aspnet.Jasmine
{
    class ResourceAssetCacheValidator : IAssetCacheValidator
    {
        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            return true;
        }
    }
}