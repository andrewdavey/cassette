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
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class JavaScriptCommentParser_Tests
    {
        [Fact]
        public void WhenParseSingleLineComment_ThenReturnOneComment()
        {
            var parser = new JavaScriptCommentParser();
            var comment = parser.Parse("// text").Single();
            comment.LineNumber.ShouldEqual(1);
            comment.Value.ShouldEqual(" text");
        }

        [Fact]
        public void WhenParseTwoSingleLineComments_ThenReturnTwoComments()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("// text1\r\n// text2").ToArray();
            comments[0].LineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual(" text1");
            comments[1].LineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual(" text2");
        }

        [Fact]
        public void WhenParseTwoSingleLineCommentsSeperatedByUnixNewLine_ThenReturnTwoComments()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("// text1\n// text2").ToArray();
            comments[0].LineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual(" text1");
            comments[1].LineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual(" text2");
        }

        [Fact]
        public void WhenParseMultilineComment_ThenReturnCommentPerLine()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("/*text1\r\ntext2*/").ToArray();
            comments[0].LineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].LineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }

        [Fact]
        public void WhenParseMultilineCommentWithUnixNewLines_ThenReturnCommentPerLine()
        {
            var parser = new JavaScriptCommentParser();
            var comments = parser.Parse("/*text1\ntext2*/").ToArray();
            comments[0].LineNumber.ShouldEqual(1);
            comments[0].Value.ShouldEqual("text1");
            comments[1].LineNumber.ShouldEqual(2);
            comments[1].Value.ShouldEqual("text2");
        }
    }
}
