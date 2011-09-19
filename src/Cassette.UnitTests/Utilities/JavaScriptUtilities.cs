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

using Should;
using Xunit;

namespace Cassette.Utilities
{
    public class JavaScriptUtilities_EscapeJavaScriptString_tests
    {
        [Fact]
        public void Backslash_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\\").ShouldEqual("\\\\");
        }

        [Fact]
        public void Double_quote_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\"").ShouldEqual("\\\"");
        }

        [Fact]
        public void Single_quote_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("'").ShouldEqual("\\'");
        }

        [Fact]
        public void Carraige_return_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\r").ShouldEqual("\\r");
        }

        [Fact]
        public void Line_feed_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("\n").ShouldEqual("\\n");
        }

        [Fact]
        public void Complex_string_escaped()
        {
            JavaScriptUtilities.EscapeJavaScriptString("'this is a \\\" test'")
                .ShouldEqual("\\'this is a \\\\\\\" test\\'");
        }
    }
}

