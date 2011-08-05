using System;
using System.IO;

namespace Cassette
{
    public interface IAsset
    {
        string SourceFilename { get; }
        void AddReference(string path);
        void AddAssetTransformer(IAssetTransformer transformer);
        Stream OpenStream();
    }
}
