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
