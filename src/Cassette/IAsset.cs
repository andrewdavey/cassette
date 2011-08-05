using System;
using System.IO;

namespace Cassette
{
    public interface IAsset
    {
        void AddAssetTransformer(IAssetTransformer transformer);
    }
}
