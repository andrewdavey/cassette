using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette
{
    class ReferenceParser
    {
        enum State { None, InSingleQuote, InDoubleQuote, InRawPath }

        public class ParsedReference
        {
            public string Path;
            public int LineNumber;
        }

        readonly ICommentParser commentParser;

        public ReferenceParser(ICommentParser commentParser)
        {
            this.commentParser = commentParser;
        }

        public IEnumerable<ParsedReference> Parse(string code, IAsset asset)
        {
            var comments = commentParser.Parse(code);

            return
                from comment in comments
                from path in ParsePaths(comment.Value, asset, comment.LineNumber)
                select new ParsedReference { Path = path, LineNumber = comment.LineNumber };
        }

        protected virtual IEnumerable<string> ParsePaths(string comment, IAsset sourceAsset, int lineNumber)
        {
            comment = comment.Trim().TrimEnd(';');
            if (!comment.StartsWith("@reference")) yield break;
            
            var state = State.None;
            var pathStart = 0;
            for (var i = 10; i < comment.Length; i++)
            {
                var c = comment[i];
                switch (state)
                {
                    case State.None:
                        if (char.IsWhiteSpace(c)) continue;
                        else if (c == '"')
                        {
                            state = State.InDoubleQuote;
                            pathStart = i + 1;
                        }
                        else if (c == '\'')
                        {
                            state = State.InSingleQuote;
                            pathStart = i + 1;
                        }
                        else
                        {
                            state = State.InRawPath;
                            pathStart = i;
                        }
                        break;

                    case State.InSingleQuote:
                        if (c == '\'')
                        {
                            yield return comment.Substring(pathStart, i - pathStart);
                            state = State.None;
                        }
                        break;

                    case State.InDoubleQuote:
                        if (c == '"')
                        {
                            yield return comment.Substring(pathStart, i - pathStart);
                            state = State.None;
                        }
                        break;

                    case State.InRawPath:
                        if (char.IsWhiteSpace(c))
                        {
                            yield return comment.Substring(pathStart, i - pathStart);
                            state = State.None;
                        }
                        break;
                }
            }
            if (state == State.InRawPath)
            {
                yield return comment.Substring(pathStart);
            }
            else if (state == State.InDoubleQuote)
            {
                throw new Exception(string.Format("Asset reference error in {0} line {1}. Missing closing double quote (\").", sourceAsset.SourceFile.FullPath, lineNumber));
            }
            else if (state == State.InSingleQuote)
            {
                throw new Exception(string.Format("Asset reference error in {0} line {1}. Missing closing single quote (').", sourceAsset.SourceFile.FullPath, lineNumber));
            }
        }
    }
}