using System;
using System.Text.RegularExpressions;
using Should;
using Xunit;

namespace Cassette
{
    public class PlaceholderTracker_Tests
    {
        [Fact]
        public void WhenInsertPlaceholder_ThenPlaceholderIdHtmlReturned()
        {
            var tracker = new PlaceholderTracker();
            var result = tracker.InsertPlaceholder(() => (""));

            var guidRegex = new Regex(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
            var output = result;
            guidRegex.IsMatch(output).ShouldBeTrue();
        }

        [Fact]
        public void GivenInsertPlaceholder_WhenReplacePlaceholders_ThenHtmlInserted()
        {
            var tracker = new PlaceholderTracker();
            var html = tracker.InsertPlaceholder(() => ("<p>test</p>"));

            tracker.ReplacePlaceholders(html).ShouldEqual(
                Environment.NewLine + "<p>test</p>" + Environment.NewLine
            );
        }
    }
}