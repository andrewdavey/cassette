using System;
using Cassette.Caching;

namespace Cassette.RequireJS
{
    public class AlwaysValid : IAssetCacheValidator
    {
        public bool IsValid(string assetPath, DateTime asOfDateTime)
        {
            return true;
        }
    }
}