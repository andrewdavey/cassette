using System.Collections.Generic;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    class CoffeeScriptCommentParser : ICommentParser
    {
        enum State
        {
            Code, SingleLineComment, MultiLineComment
        }

        public IEnumerable<Comment> Parse(string code)
        {
            var state = State.Code;
            var line = 1;
            var commentStart = 0;

            for (int i = 0; i < code.Length; i++)
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
                        if (c == '#')
                        {
                            state = State.SingleLineComment;

                            if (i < code.Length - 2 && code[i + 1] == '#' && code[i + 2] == '#')
                            {
                                state = State.MultiLineComment;
                                i += 2;
                            }
                            commentStart = i + 1;
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
                        // Scan forwards until "###" or end of code.
                        while (i < code.Length - 2 && (code[i] != '#' || code[i + 1] != '#' || code[i + 2] != '#'))
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
                        i += 2; // Skip the '###'
                        state = State.Code;
                        break;
                }
            }
        }
    }
}
