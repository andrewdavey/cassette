using System.IO;

namespace Cassette
{
    public interface IUnresolvedAssetParser
    {
        UnresolvedAsset Parse(Stream source, string sourcePath);
    }
}
