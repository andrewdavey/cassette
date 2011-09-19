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

using System;
using System.Text.RegularExpressions;
using System.Web;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class PlaceholderTracker_Tests
    {
        [Fact]
        public void WhenInsertPlaceholder_ThenPlaceholderIdHtmlReturned()
        {
            var tracker = new PlaceholderTracker();
            var result = tracker.InsertPlaceholder(() => new HtmlString(""));

            var guidRegex = new Regex(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
            var output = result.ToHtmlString();
            guidRegex.IsMatch(output).ShouldBeTrue();
        }

        [Fact]
        public void GivenInsertPlaceholder_WhenReplacePlaceholders_ThenHtmlInserted()
        {
            var tracker = new PlaceholderTracker();
            var html = tracker.InsertPlaceholder(() => new HtmlString("<p>test</p>"));

            tracker.ReplacePlaceholders(html.ToHtmlString()).ShouldEqual(
                Environment.NewLine + "<p>test</p>" + Environment.NewLine
            );
        }
    }
}

