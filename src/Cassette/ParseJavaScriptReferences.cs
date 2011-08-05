using System.IO;
using System.Text.RegularExpressions;

namespace Cassette
{
    public class ParseJavaScriptReferences : ModuleProcessorOfAssetsMatchingFileExtension<Module>
    {
        public ParseJavaScriptReferences() : base("js") { }

        readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* [""'](.*?)[""'] \s* /?>",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        protected override void Process(IAsset asset)
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
    }
}
