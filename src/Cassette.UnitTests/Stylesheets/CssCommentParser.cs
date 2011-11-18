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
