﻿using System.Collections.Generic;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    class CssCommentParser : ICommentParser
    {
        enum State
        {
            Code, Comment
        }

        public IEnumerable<Comment> Parse(string code)
        {
            var state = State.Code;
            var commentStart = 0;
            var line = 1;
            for (var i = 0; i < code.Length; i++)
            {
                var c = code[i];

                if (c == '\r')
                {
                    if (i < code.Length - 1 && code[i + 1] == '\n')
                    {
                        i++;
                    }
                    line++;
                    continue;
                }
                else if (c == '\n')
                {
                    line++;
                    continue;
                }

                switch (state)
                {
                    case State.Code:
                        if (c != '/') continue;
                        if (i >= code.Length - 2) yield break;
                        if (code[i + 1] == '*')
                        {
                            state = State.Comment;
                            commentStart = i + 2;
                            i++; // Skip the '*'
                        }
                        break;

                    case State.Comment:
                        // Scan forwards until "*/" or end of code.
                        while (i < code.Length - 1 && (code[i] != '*' || code[i + 1] != '/'))
                        {
                            // Track new lines within the comment.
                            switch (code[i])
                            {
                                case '\r':
                                    yield return new Comment
                                    {
                                        LineNumber = line,
                                        Value = code.Substring(commentStart, i - commentStart)
                                    };
                                    i++;
                                    if (i < code.Length && code[i] == '\n')
                                    {
                                        i++;
                                    }
                                    commentStart = i;
                                    line++;
                                    break;

                                case '\n':
                                    yield return new Comment
                                    {
                                        LineNumber = line,
                                        Value = code.Substring(commentStart, i - commentStart)
                                    };
                                    i++;
                                    commentStart = i;
                                    line++;
                                    break;

                                default:
                                    i++;
                                    break;
                            }
                        }
                        yield return new Comment
                        {
                            LineNumber = line,
                            Value = code.Substring(commentStart, i - commentStart)
                        };
                        i++; // Skip the '/'
                        state = State.Code;
                        break;
                }
            }
        }
    }
}
