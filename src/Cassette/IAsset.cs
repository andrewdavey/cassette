using System.Collections.Generic;
using System.IO;
using Cassette.IO;

namespace Cassette
{
    public interface IAsset
    {
        void Accept(IAssetVisitor visitor);
        string SourceFilename { get; }
        byte[] Hash { get; }
        IEnumerable<AssetReference> References { get; }
        void AddReference(string path, int lineNumber);
        void AddAssetTransformer(IAssetTransformer transformer);
        Stream OpenStream();
        IDirectory Directory { get; }
        void AddRawFileReference(string relativeFilename);
    }
}
