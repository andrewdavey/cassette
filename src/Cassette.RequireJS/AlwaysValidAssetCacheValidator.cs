using System;
using Cassette.Caching;

namespace Cassette.RequireJS
{
    public class AlwaysValidAssetCacheValidator : IAssetCacheValidator
    {
        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            return true;
        }
    }
}