using System.Collections.Generic;

namespace Cassette
{
    public class AssetPathComparer : IEqualityComparer<IAsset>
    {
        public bool Equals(IAsset x, IAsset y)
        {
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(IAsset obj)
        {
            return obj.Path.GetHashCode();
        }
    }
}