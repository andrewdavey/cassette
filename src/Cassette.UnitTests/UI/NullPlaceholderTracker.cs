using System.Web;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class NullPlaceholderTracker_Tests
    {
        [Fact]
        public void InsertPlaceholderCallsFuncAndReturnsTheResult()
        {
            var tracker = new NullPlaceholderTracker();
            var result = tracker.InsertPlaceholder(() => new HtmlString("<p></p>"));
            result.ToHtmlString().ShouldEqual("<p></p>");
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