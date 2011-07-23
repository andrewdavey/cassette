using System.IO;
using Cassette.ModuleBuilding;
using Cassette.Utilities;

namespace Cassette.Assets.HtmlTemplates
{
    public class UnresolvedHtmlTemplateParser : IUnresolvedAssetParser
    {
        public UnresolvedAsset Parse(Stream source, string sourcePath)
        {
            return new UnresolvedAsset(
                sourcePath,
                source.ComputeSHA1Hash(),
                new string[0] // HTML templates do not support references, for now.
            );
        }
    }
}
