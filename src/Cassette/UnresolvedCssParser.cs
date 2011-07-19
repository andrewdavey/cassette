using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette
{
    public class UnresolvedCssParser : IUnresolvedResourceParser
    {
        readonly string applicationRoot;
        readonly Regex cssCommentRegex = new Regex(
            @"/\*(?<body>.*?)\*/",
            RegexOptions.Singleline
        );
        readonly Regex referenceRegex = new Regex(
            @"@reference \s+ (?<quote>[""']) (?<path>.*?) \<quote> \s* ;",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );

        public UnresolvedCssParser(string applicationRoot)
        {
            this.applicationRoot = applicationRoot;
        }

        public UnresolvedResource Parse(Stream source, string sourcePath)
        {
            return new UnresolvedResource(
                sourcePath,
                Hash(source),
                ParseReferences(source).ToArray()
            );
        }

        byte[] Hash(Stream source)
        {
            // A stylesheet may have its URL values rewritten into absolute form.
            // This makes the stylesheet depend on the application root path.
            // Therefore we include this in the hash so if the application root changes we
            // invalidate the cached modules.
            using (var memory = new MemoryStream())
            {
                source.CopyTo(memory);
                var rootBytes = Encoding.Unicode.GetBytes(applicationRoot);
                memory.Write(rootBytes, 0, rootBytes.Length);
                memory.Position = 0;

                return memory.ComputeSHA1Hash();
            }
        }

        IEnumerable<string> ParseReferences(Stream source)
        {
            // Cassette looks inside CSS comments for special "reference" declarations.
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
