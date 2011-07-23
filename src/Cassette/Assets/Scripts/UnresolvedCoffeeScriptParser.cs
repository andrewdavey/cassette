using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;
using Cassette.ModuleBuilding;

namespace Cassette.Assets.Scripts
{
    public class UnresolvedCoffeeScriptParser : IUnresolvedAssetParser
    {
        public UnresolvedAsset Parse(Stream source, string sourcePath)
        {
            return new UnresolvedAsset(
                sourcePath,
                source.ComputeSHA1Hash(),
                ParseReferences(source).ToArray()
            );
        }

        readonly Regex referenceRegex = new Regex(
            @"#\s*reference\s+[""'](.*)[""']",
            RegexOptions.IgnoreCase
        );

        IEnumerable<string> ParseReferences(Stream source)
        {
            source.Position = 0;
            using (var reader = new StreamReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var match = referenceRegex.Match(line);
                    if (match.Success)
                    {
                        var path = match.Groups[1].Value;
                        yield return path;
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
