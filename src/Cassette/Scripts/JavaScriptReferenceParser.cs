using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cassette.Scripts
{
    class JavaScriptReferenceParser : ReferenceParser
    {
        static readonly Regex ReferenceRegex = new Regex(
            @"\<reference \s+ path \s* = \s* (?<quote>[""']) (?<path>.*?) \<quote> \s* /?>",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );

        public JavaScriptReferenceParser(ICommentParser commentParser)
            : base(commentParser)
        {
        }

        protected override IEnumerable<string> ParsePaths(string comment, IAsset sourceAsset, int lineNumber)
        {
            var simplePaths = base.ParsePaths(comment, sourceAsset, lineNumber);
            var xmlCommentPaths = ParseXmlDocCommentPaths(comment);
            return simplePaths.Concat(xmlCommentPaths);
        }

        static IEnumerable<string> ParseXmlDocCommentPaths(string comment)
        {
            return ReferenceRegex
                .Matches(comment)
                .Cast<Match>()
                .Select(m => m.Groups["path"].Value);
        }
    }
}