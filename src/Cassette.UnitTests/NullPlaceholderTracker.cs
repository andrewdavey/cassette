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

namespace Cassette
{
    public class NullPlaceholderTracker_Tests
    {
        [Fact]
        public void InsertPlaceholderCallsFuncAndReturnsTheResult()
        {
            var tracker = new NullPlaceholderTracker();
            var result = tracker.InsertPlaceholder(() => ("<p></p>"));
            result.ShouldEqual("<p></p>");
        }

        [Fact]
        public void ReplacePlaceholdersReturnsTheHtmlPassedToIt()
        {
            var tracker = new NullPlaceholderTracker();
            var result = tracker.ReplacePlaceholders("<p></p>");
            result.ShouldEqual("<p></p>");
        }
    }
}
