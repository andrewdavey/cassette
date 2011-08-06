using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    public interface IAsset
    {
        string SourceFilename { get; }
        IEnumerable<AssetReference> References { get; }
        void AddReference(string path);
        void AddAssetTransformer(IAssetTransformer transformer);
        Stream OpenStream();
    }
}
