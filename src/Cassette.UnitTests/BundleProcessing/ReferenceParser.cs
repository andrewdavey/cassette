using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ReferenceParser_Test
    {
        readonly ReferenceParser parser = new ReferenceParser(new JavaScriptCommentParser());
        readonly IAsset asset = Mock.Of<IAsset>();

        [Fact]
        public void CanParseSimpleReference()
        {
            var javascript = "// @reference ~/path";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path");
            references[0].LineNumber.ShouldEqual(1);
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
            references[0].LineNumber.ShouldEqual(1);
            references[1].Path.ShouldEqual("~/path2");
            references[1].LineNumber.ShouldEqual(2);
        }

        [Fact]
        public void CanParseReferencesInMultilineComment()
        {
            var javascript = "/* @reference ~/path1\r\n@reference ~/path2*/";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[1].Path.ShouldEqual("~/path2");
            references[1].LineNumber.ShouldEqual(2);
        }

        [Fact]
        public void IgnoresTrailingSemicolonInComment()
        {
            var javascript = "// @reference ~/path;";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path");
        }

        [Fact]
        public void MatchesDoubleQuotesButReturnsOnlyTheContent()
        {
            var javascript = "// @reference \"~/path\"";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path");
        }

        [Fact]
        public void MatchesSingleQuotesButReturnsOnlyTheContent()
        {
            var javascript = "// @reference '~/path'";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path");
        }

        [Fact]
        public void MatchesMultipleQuotedPathsButReturnsOnlyTheContent()
        {
            var javascript = "// @reference '~/path1' '~/path2'";
            var references = parser.Parse(javascript, asset).ToArray();

            references[0].Path.ShouldEqual("~/path1");
            references[1].Path.ShouldEqual("~/path2");
        }
    }
}
