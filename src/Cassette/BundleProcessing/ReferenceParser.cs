#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.BundleProcessing
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
