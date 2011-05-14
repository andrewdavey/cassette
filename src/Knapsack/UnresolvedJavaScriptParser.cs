using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;
using Knapsack.Utilities;

namespace Knapsack
{
    public class UnresolvedJavaScriptParser : IUnresolvedResourceParser
    {
        public UnresolvedResource Parse(Stream source, string sourcePath)
        {
            return new UnresolvedResource(
                sourcePath, 
                source.ComputeSHA1Hash(), 
                ParseReferences(source).ToArray()
            );
        }

        readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* [""'](.*)[""'] \s* />", 
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
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
