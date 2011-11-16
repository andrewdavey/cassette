using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class JavaScriptCommentParser_Tests
    {
        [Fact]
        public void WhenParseSingleLineComment_ThenReturnOneComment()
        {
            var parser = new JavaScriptCommentParser();
            var comment = parser.Parse("// text").Single();
            comment.SourceLineNumber.ShouldEqual(1);
            comment.Value.ShouldEqual(" text");
        }

        [Fact]
        public void WhenParseTwoSingleLineComments_ThenReturnTwoComments()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("// text1\r\n// text2").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual(" text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual(" text2");
        }

        [Fact]
        public void WhenParseTwoSingleLineCommentsSeperatedByUnixNewLine_ThenReturnTwoComments()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("// text1\n// text2").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual(" text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual(" text2");
        }

        [Fact]
        public void WhenParseMultilineComment_ThenReturnCommentPerLine()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("/*text1\r\ntext2*/").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }

        [Fact]
        public void WhenParseMultilineCommentWithUnixNewLines_ThenReturnCommentPerLine()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("/*text1\ntext2*/").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }
    }
    
    public class CssCommentParser_Tests
    {
        [Fact]
        public void WhenParseMultilineComment_ThenReturnCommentPerLine()
        {
            var parser = new CssCommentParser();
            var comments = parser.Parse("/*text1\r\ntext2*/").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }

        [Fact]
        public void WhenParseMultilineCommentWithUnixNewLines_ThenReturnCommentPerLine()
        {
            var parser = new CssCommentParser();
            var comments = parser.Parse("/*text1\ntext2*/").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }
    }

    public class HtmlTemplateCommentParser_Tests
    {
        readonly HtmlTemplateCommentParser parser = new HtmlTemplateCommentParser();
        
        [Fact]
        public void WhenParseEmptyComment_ThenReturnCommentWithEmptyValue()
        {
            parser.Parse("<!---->").Single().Value.ShouldEqual("");
        }

        [Fact]
        public void WhenParseHtmlComment_ThenReturnComment()
        {
            var comment = parser.Parse("<!-- text -->").Single();
            comment.SourceLineNumber.ShouldEqual(1);
            comment.Value.ShouldEqual(" text ");
        }

        [Fact]
        public void WhenParseHtmlCommentWithNewLines_ThenReturnCommentPerLine()
        {
            var comments = parser.Parse("<!--text1\r\ntext2-->").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }

        [Fact]
        public void WhenParseHtmlCommentWithUnixNewLines_ThenReturnCommentPerLine()
        {
            var comments = parser.Parse("<!--text1\ntext2-->").ToArray();
            comments[0].SourceLineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].SourceLineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }
    }

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

    public class CoffeeScriptCommentParser_Tests
    {
        
    }
    
    public class ReferenceParser_Test
    {
        ReferenceParser parser = new ReferenceParser(new JavaScriptCommentParser());
        IAsset asset = Mock.Of<IAsset>();

        [Fact]
        public void CanParseSimpleReference()
        {
            var javascript = "// @reference ~/path";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path");
            references[0].SourceLineNumber.ShouldEqual(1);
            references[0].Type.ShouldEqual(AssetReferenceType.SameBundle);
        }

        [Fact]
        public void CanParseTwoReferencesOnOneLine()
        {
            var javascript = "// @reference ~/path1 ~/path2";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[1].Path.ShouldEqual("~/path2");
        }

        [Fact]
        public void CanParseReferencesOnTwoLines()
        {
            var javascript = "// @reference ~/path1\r\n// @reference ~/path2";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[0].SourceLineNumber.ShouldEqual(1);
            references[1].Path.ShouldEqual("~/path2");
            references[1].SourceLineNumber.ShouldEqual(2);
        }

        [Fact]
        public void CanParseReferencesInMultilineComment()
        {
            var javascript = "/* @reference ~/path1\r\n@reference ~/path2*/";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[1].Path.ShouldEqual("~/path2");
            references[1].SourceLineNumber.ShouldEqual(2);
        }

        [Fact]
        public void IgnoresTrailingSemicolonInComment()
        {
            var javascript = "// @reference ~/path1;";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
        }
    }

    public class JavaScriptCommentParser : ICommentParser
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
                    i++;
                    if (i < code.Length && code[i] == '\n')
                    {
                        i++;
                    }
                    line++;
                    continue;
                }
                else if (c == '\n')
                {
                    i++;
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
                            SourceLineNumber = line,
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
                                    SourceLineNumber = line,
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
                                    SourceLineNumber = line,
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
                            SourceLineNumber = line,
                            Value = code.Substring(commentStart, i - commentStart)
                        };
                        i++; // Skip the '/'
                        state = State.Code;
                        break;
                }
            }
        }
    }

    public class CssCommentParser : ICommentParser
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
                    i++;
                    if (i < code.Length && code[i] == '\n')
                    {
                        i++;
                    }
                    line++;
                    continue;
                }
                else if (c == '\n')
                {
                    i++;
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
                            if (code[i] == '\r')
                            {
                                yield return new Comment
                                {
                                    SourceLineNumber = line,
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
                                    SourceLineNumber = line,
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
                            SourceLineNumber = line,
                            Value = code.Substring(commentStart, i - commentStart)
                        };
                        i++; // Skip the '/'
                        state = State.Code;
                        break;
                }
            }
        }
    }

    public class Comment
    {
        public int SourceLineNumber;
        public string Value;
    }

    public interface ICommentParser
    {
        IEnumerable<Comment> Parse(string code);
    }

    public class ReferenceParser
    {
        readonly ICommentParser commentParser;

        public ReferenceParser(ICommentParser commentParser)
        {
            this.commentParser = commentParser;
        }

        public IEnumerable<AssetReference> Parse(string code, IAsset sourceAsset)
        {
            var comments = commentParser.Parse(code);
            return from comment in comments
                   from reference in ParseReferences(comment.SourceLineNumber, comment.Value, sourceAsset)
                   select reference;
        }

        IEnumerable<AssetReference> ParseReferences(int lineNumber, string comment, IAsset sourceAsset)
        {
            var lines = comment.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return from line in lines
                   let parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   from path in parts.Skip(1)
                   select new AssetReference(path.TrimEnd(';'), sourceAsset, lineNumber, AssetReferenceType.SameBundle);            
        }
    }
}