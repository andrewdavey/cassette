using System.Collections.Generic;

namespace Cassette.Scripts
{
    class JavaScriptCommentParser : ICommentParser
    {
        enum State
        {
            Code, SingleLineComment, MultiLineComment
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
                        if (code[i + 1] == '/')
                        {
                            state = State.SingleLineComment;
                            commentStart = i + 2;
                            i++; // Skip the '/'
                        }
                        else if (code[i + 1] == '*')
                        {
                            state = State.MultiLineComment;
                            commentStart = i + 2;
                            i++; // Skip the '*'
                        }
                        break;

                    case State.SingleLineComment:
                        // Scan forward until newline or end of code.
                        while (i < code.Length && code[i] != '\r' && code[i] != '\n')
                        {
                            i++;
                        }
                        yield return new Comment
                        {
                            LineNumber = line,
                            Value = code.Substring(commentStart, i - commentStart)
                        };
                        if (i < code.Length - 1 && code[i] == '\r' && code[i + 1] == '\n') i++;
                        line++;
                        state = State.Code;
                        break;

                    case State.MultiLineComment:
                        // Scan forwards until "*/" or end of code.
                        while (i < code.Length - 1 && (code[i] != '*' || code[i + 1] != '/'))
                        {
                            // Track new lines within the comment.
                            if (code[i] == '\r')
                            {
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
                                continue;
                            }
                            else if (code[i] == '\n')
                            {
                                yield return new Comment
                                {
                                    LineNumber = line,
                                    Value = code.Substring(commentStart, i - commentStart)
                                };
                                i++;
                                commentStart = i;
                                line++;
                                continue;
                            }
                            else
                            {
                                i++;
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