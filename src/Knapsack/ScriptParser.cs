using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Knapsack
{
    public class ScriptParser
    {
        public Script Parse(Stream source, string sourcePath)
        {
            return new Script(
                sourcePath, 
                Hash(source), 
                ExpandPaths(ParseReferences(source), sourcePath).ToArray()
            );
        }

        readonly Regex referenceRegex = new Regex(
            @"/// \s* \<reference \s+ path \s* = \s* [""'](.*)[""'] \s* />", 
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );
        readonly Regex pathSplitter = new Regex(@"\\|/");
        readonly string rootDirectory;

        byte[] Hash(Stream source)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(source);
            }
        }

        IEnumerable<string> ExpandPaths(IEnumerable<string> relativePaths, string sourcePath)
        {
            var currentDirectory = Path.GetDirectoryName(sourcePath);
            var currentDirectoryNames = pathSplitter.Split(currentDirectory);
            foreach (var path in relativePaths)
            {
                yield return NormalizePath(currentDirectoryNames, path);
            }
        }

        string NormalizePath(string[] currentDirectory, string path)
        {
            // ("app", "~/foo/bar.js") -> "foo/bar.js" 
            if (path.StartsWith("~")) return path.Substring(2);

            // ("app/sub", "../foo/bar.js") -> "app/foo/bar.js"
            var names = currentDirectory.Concat(pathSplitter.Split(path));
            var stack = new Stack<string>();
            foreach (var name in names)
            {
                if (name == "..")
                {
                    stack.Pop();
                }
                else
                {
                    stack.Push(name);
                }
            }
            return string.Join("\\", stack.Reverse());
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
