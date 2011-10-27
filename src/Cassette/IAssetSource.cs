using System.Collections.Generic;
using Cassette.IO;

namespace Cassette
{
    public interface IAssetSource
    {
        IEnumerable<IAsset> GetAssets(IDirectory directory, Bundle bundle);
    }
}