using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Cassette
{
    public interface IAsset
    {
        string SourceFilename { get; }
        IEnumerable<AssetReference> References { get; }
        void AddReference(string path, int lineNumber);
        void AddAssetTransformer(IAssetTransformer transformer);
        Stream OpenStream();
        bool IsFrom(string filename);
        IEnumerable<XElement> CreateManifest();
    }
}
