using System;
using System.Collections.Generic;

namespace Cassette
{
    class AssetReferenceFilenameEqualityComparer : IEqualityComparer<AssetReference>
    {
        public bool Equals(AssetReference x, AssetReference y)
        {
            return x.ReferencedFilename.Equals(y.ReferencedFilename, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(AssetReference obj)
        {
            return obj.ReferencedFilename.GetHashCode();
        }
    }
}
