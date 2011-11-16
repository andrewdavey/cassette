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

    public class Comment
    {
        public int SourceLineNumber;
        public string Value;
    }

    public class JavaScriptCommentParser
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

    public class JavaScriptReferenceParser_Test
    {
        [Fact]
        public void CanParseSimpleReference()
        {
            var javascript = "// @reference ~/path";
            var asset = Mock.Of<IAsset>();

            var parser = new JavaScriptReferenceParser();
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path");
            references[0].SourceLineNumber.ShouldEqual(1);
            references[0].Type.ShouldEqual(AssetReferenceType.SameBundle);
        }

        [Fact]
        public void CanParseTwoReferencesOnOneLine()
        {
            var javascript = "// @reference ~/path1 ~/path2";
            var asset = Mock.Of<IAsset>();

            var parser = new JavaScriptReferenceParser();
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[1].Path.ShouldEqual("~/path2");
        }

        [Fact]
        public void CanParseReferencesOnTwoLines()
        {
            var javascript = "// @reference ~/path1\r\n// @reference ~/path2";
            var asset = Mock.Of<IAsset>();

            var parser = new JavaScriptReferenceParser();
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
            var asset = Mock.Of<IAsset>();

            var parser = new JavaScriptReferenceParser();
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[1].Path.ShouldEqual("~/path2");
            references[1].SourceLineNumber.ShouldEqual(2);
        }

        [Fact]
        public void IgnoresTrailingSemicolonInComment()
        {
            var javascript = "// @reference ~/path1;";
            var asset = Mock.Of<IAsset>();

            var parser = new JavaScriptReferenceParser();
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
        }
    }

    public abstract class CommonReferenceParser
    {
        public IEnumerable<AssetReference> Parse(string code, IAsset sourceAsset)
        {
            var comments = ParseComments(code);
            return from comment in comments
                   from reference in ParseReferences(comment.Item1, comment.Item2, sourceAsset)
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

        protected abstract IEnumerable<Tuple<int, string>> ParseComments(string code);
    }

    public class JavaScriptReferenceParser : CommonReferenceParser
    {
        enum State
        {
            Code, SingleLineComment, MultiLineComment
        }

        protected override IEnumerable<Tuple<int, string>> ParseComments(string code)
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
                        if (i >= code.Length-2) yield break;
                        if (code[i+1] == '/')
                        {
                            state = State.SingleLineComment;
                            commentStart = i + 2;
                            i++; // Skip the '/'
                        }
                        else if (code[i+1] == '*')
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
                        yield return Tuple.Create(line, code.Substring(commentStart, i - commentStart));
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
                                i++;
                                if (i < code.Length && code[i] == '\n')
                                {
                                    i++;
                                }
                                line++;
                                continue;
                            }
                            else if (code[i] == '\n')
                            {
                                i++;
                                line++;
                                continue;
                            }
                            else
                            {
                                i++;                                
                            }
                        }
                        yield return Tuple.Create(line, code.Substring(commentStart, i - commentStart));
                        i++; // Skip the '/'
                        state = State.Code;
                        break;
                }
            }
        }
    }
}
