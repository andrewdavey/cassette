using System;
using System.IO;

namespace Cassette
{
    public interface IAsset
    {
        string SourceFilename { get; }
        void AddAssetTransformer(IAssetTransformer transformer);
        Stream OpenStream();
    }
}
