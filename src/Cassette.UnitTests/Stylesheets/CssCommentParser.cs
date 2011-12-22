using System.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class CssCommentParser_Tests
    {
        [Fact]
        public void WhenParseMultilineComment_ThenReturnCommentPerLine()
        {
            var parser = new CssCommentParser();
            var comments = parser.Parse("/*text1\r\ntext2*/").ToArray();
            comments[0].LineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].LineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }

        [Fact]
        public void WhenParseMultilineCommentWithUnixNewLines_ThenReturnCommentPerLine()
        {
            var parser = new CssCommentParser();
            var comments = parser.Parse("/*text1\ntext2*/").ToArray();
            comments[0].LineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].LineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }

        [Fact]
        public void WhenNewLinesBeforeComments_ThenReturnCommentsWithCorrectLineNumbers()
        {
            var parser = new CssCommentParser();
            var comments = parser.Parse("\r\n/*text1*/\r\n\r\n/*text2*/").ToArray();
            comments[0].LineNumber.ShouldEqual(2);
            comments[0].Value.ShouldEqual("text1");
            comments[1].LineNumber.ShouldEqual(4);
            comments[1].Value.ShouldEqual("text2");
        }
    }
}
