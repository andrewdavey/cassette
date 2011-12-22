using System.Collections.Generic;
using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateCommentParser : ICommentParser
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
            for (var i = 0; i <= code.Length - 3; i++)
            {
                switch (state)
                {
                    case State.Code:
                        if (code[i] == '\r')
                        {
                            if (i < code.Length - 1 && code[i + 1] == '\n')
                            {
                                i++;
                            }
                            line++;
                            continue;
                        }
                        else if (code[i] == '\n')
                        {
                            line++;
                            continue;
                        }
                        else if (i < code.Length - 4 && code.Substring(i, 4) == "<!--")
                        {
                            state = State.Comment;
                            i += 3;
                            commentStart = i + 1;
                        }
                        break;

                    case State.Comment:
                        if (code.Substring(i, 3) == "-->")
                        {
                            yield return new Comment
                            {
                                LineNumber = line,
                                Value = code.Substring(commentStart, i - commentStart)
                            };
                            i += 2;
                            state = State.Code;
                        }
                        else if (code[i] == '\r')
                        {
                            if (i < code.Length - 1 && code[i + 1] == '\n')
                            {
                                yield return new Comment
                                {
                                    LineNumber = line,
                                    Value = code.Substring(commentStart, i - commentStart)
                                };
                                i++;
                                commentStart = i + 1;
                            }
                            line++;
                        }
                        else if (code[i] == '\n')
                        {
                            yield return new Comment
                            {
                                LineNumber = line,
                                Value = code.Substring(commentStart, i - commentStart)
                            };
                            i++;
                            line++;
                            commentStart = i;
                        }
                        break;
                }
            }
        }
    }
}
