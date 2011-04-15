using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Knapsack
{
    public class ScriptParser
    {
        public UnresolvedScript Parse(Stream source, string sourcePath)
        {
            return new UnresolvedScript(
                sourcePath, 
                Hash(source), 
                ParseReferences(source).ToArray()
            );
        }

        readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* [""'](.*)[""'] \s* />", 
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        byte[] Hash(Stream source)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(source);
            }
        }

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
