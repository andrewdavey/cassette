using System.IO;

namespace Cassette.ModuleBuilding
{
    public interface IUnresolvedAssetParser
    {
        UnresolvedAsset Parse(Stream source, string sourcePath);
    }
}
