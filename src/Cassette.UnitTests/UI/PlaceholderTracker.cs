using System;
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
            var result = tracker.InsertPlaceholder("id", () => new HtmlString(""));
            result.ToHtmlString().ShouldEqual(
                Environment.NewLine + "id" + Environment.NewLine
            );
        }

        [Fact]
        public void GivenInsertPlaceholder_WhenReplacePlaceholders_ThenHtmlInserted()
        {
            var tracker = new PlaceholderTracker();
            var html = tracker.InsertPlaceholder("id", () => new HtmlString("<p>test</p>"));

            tracker.ReplacePlaceholders(html.ToHtmlString()).ShouldEqual(
                Environment.NewLine + "<p>test</p>" + Environment.NewLine
            );
        }
    }
}
