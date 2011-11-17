using System.Collections.Generic;

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
                        if (code.Substring(i, 4) == "<!--")
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
                                SourceLineNumber = line,
                                Value = code.Substring(commentStart, i - commentStart)
                            };
                            i += 2;
                            state = State.Code;
                        }
                        else if (code[i] == '\r')
                        {
                            i++;
                            if (i < code.Length && code[i] == '\n')
                            {
                                yield return new Comment
                                {
                                    SourceLineNumber = line,
                                    Value = code.Substring(commentStart, i - commentStart - 1)
                                };
                                i++;
                                commentStart = i;
                            }
                            line++;
                        }
                        else if (code[i] == '\n')
                        {
                            yield return new Comment
                            {
                                SourceLineNumber = line,
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