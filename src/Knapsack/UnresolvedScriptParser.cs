using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;

namespace Knapsack
{
    public class UnresolvedScriptParser
    {
        public UnresolvedScript Parse(Stream source, string sourcePath)
        {
            var isCoffeeScript = sourcePath.EndsWith(".coffee", StringComparison.InvariantCulture);
            return new UnresolvedScript(
                sourcePath, 
                Hash(source), 
                ParseReferences(source, isCoffeeScript).ToArray()
            );
        }

        readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* [""'](.*)[""'] \s* />", 
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );
        readonly Regex coffeeReferenceRegex = new Regex(
            @"#\s*reference\s+[""'](.*)[""']",
            RegexOptions.IgnoreCase
        );

        byte[] Hash(Stream source)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(source);
            }
        }

        IEnumerable<string> ParseReferences(Stream source, bool isCoffeeScript)
        {
            var regex = isCoffeeScript ? coffeeReferenceRegex : referenceRegex;
            source.Position = 0;
            using (var reader = new StreamReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var match = regex.Match(line);
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
