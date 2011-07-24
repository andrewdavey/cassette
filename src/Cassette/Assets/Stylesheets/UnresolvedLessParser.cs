using System.IO;
using Cassette.ModuleBuilding;
using Cassette.Utilities;

namespace Cassette.Assets.Stylesheets
{
    public class UnresolvedLessParser : IUnresolvedAssetParser
    {
        public UnresolvedAsset Parse(Stream source, string sourcePath)
        {
            // TODO: Parse reference comments or maybe @import statements?

            return new UnresolvedAsset(
                sourcePath,
                source.ComputeSHA1Hash(),
                new string[0]
            );
        }
    }
}
