using System.Collections.Generic;
using System.IO;

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
        IFileSystem Directory { get; }
        void AddRawFileReference(string relativeFilename);
    }
}
