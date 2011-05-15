using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Knapsack.Utilities;

namespace Knapsack
{
    public class UnresolvedCssParser : IUnresolvedResourceParser
    {
        public UnresolvedResource Parse(Stream source, string sourcePath)
        {
            return new UnresolvedResource(
                sourcePath,
                source.ComputeSHA1Hash(),
                ParseReferences(source).ToArray()
            );
        }

        readonly Regex cssCommentRegex = new Regex(
            @"/\*(?<body>.*?)\*/",
            RegexOptions.Singleline
        );

        readonly Regex referenceRegex = new Regex(
            @"@reference \s+ (?<quote>[""']) (?<path>.*?) \<quote> \s* ;",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );

        IEnumerable<string> ParseReferences(Stream source)
        {
            // Knapsack looks inside CSS comments for special "reference" declarations.
            // e.g. /* @reference "something.css"; */
            // Using @import instead was considered, but may interfere with intended behavior.
            // Also, it would require removing the @import directives from the outgoing CSS.
            // Using a comment based declaration is easier to implement and works much like
            // the JavaScript referencing system.

            source.Position = 0;
            using (var reader = new StreamReader(source))
            {
                var css = reader.ReadToEnd();

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
}
