using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
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

        readonly Regex[] importRegexes = new Regex[]
        {
            new Regex(
                @"@import \s+ (?<quote>[""']) (?<url>.*?) \<quote> \s* ;",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            ),
            new Regex(
                @"@import \s+ url \s* \( \s* (?<quote>[""']?) (?<url>.*?) \<quote> \s* \) \s* ;",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            )
        };

        IEnumerable<string> ParseReferences(Stream source)
        {
            source.Position = 0;
            using (var reader = new StreamReader(source))
            {
                var css = reader.ReadToEnd();

                return importRegexes
                    .SelectMany(regex => regex.Matches(css).Cast<Match>())
                    .Where(match => match.Groups["url"].Success)
                    .Select(match => match.Groups["url"].Value);
            }
        }
    }
}
