using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cassette
{
    public class ParseJavaScriptReferences : IModuleProcessor<Module>
    {
        public void Process(Module module)
        {
            foreach (var asset in module.Assets.Where(IsJavaScript))
            {
                ParseAssetReferences(asset);
            }
        }

        readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* [""'](.*?)[""'] \s* /?>",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        void ParseAssetReferences(IAsset asset)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var match = referenceRegex.Match(line);
                    if (match.Success)
                    {
                        asset.AddReference(match.Groups[1].Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        bool IsJavaScript(IAsset asset)
        {
            return asset.SourceFilename.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
        }
    }
}
