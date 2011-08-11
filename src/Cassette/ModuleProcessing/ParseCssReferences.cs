using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cassette.ModuleProcessing
{
    public class ParseCssReferences : ModuleProcessorOfAssetsMatchingFileExtension<StylesheetModule>
    {
        public ParseCssReferences()
            : base("css")
        {
        }

        static readonly Regex cssCommentRegex = new Regex(
            @"/\*(?<body>.*?)\*/",
            RegexOptions.Singleline
        );
        static readonly Regex referenceRegex = new Regex(
            @"@reference \s+ (?<quote>[""']) (?<path>.*?) \<quote> \s* ;",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );

        protected override void Process(IAsset asset, Module module)
        {
            var css = ReadAllCss(asset);
            foreach (var reference in ParseReferences(css))
            {
                // TODO: Add line number tracking to the parser.
                // For now use -1 as dummy line number.
                asset.AddReference(reference, -1);
            }
        }

        string ReadAllCss(IAsset asset)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                return reader.ReadToEnd();
            }
        }

        IEnumerable<string> ParseReferences(string css)
        {
            var commentBodies = cssCommentRegex
                    .Matches(css)
                    .Cast<Match>()
                    .Select(match => match.Groups["body"].Value);

            return from body in commentBodies
                   from match in referenceRegex.Matches(body).Cast<Match>()
                   where match.Groups["path"].Success
                   select match.Groups["path"].Value;
        }
    }
}
