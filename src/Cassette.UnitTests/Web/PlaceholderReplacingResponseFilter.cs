using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class PlaceholderReplacingResponseFilter_tests
    {
        MemoryStream outputStream;
        Mock<HttpResponseBase> response;
        Mock<IPlaceholderTracker> placeholderTracker;
        PlaceholderReplacingResponseFilter filter;

        public PlaceholderReplacingResponseFilter_tests()
        {
            outputStream = new MemoryStream();
            response = new Mock<HttpResponseBase>();
            placeholderTracker = new Mock<IPlaceholderTracker>();

            placeholderTracker.Setup(h => h.ReplacePlaceholders(It.IsAny<string>()))
                      .Returns<string>(s => s);

            response.SetupGet(r => r.ContentType).Returns("text/html");
            response.SetupGet(r => r.Output.Encoding).Returns(Encoding.ASCII);
            response.SetupGet(r => r.Filter).Returns(outputStream);
            filter = new PlaceholderReplacingResponseFilter(response.Object, placeholderTracker.Object);
        }

        [Fact]
        public void Write_sends_buffer_to_output_stream()
        {
            var html = "<html>\r\n";
            filter.Write(Encoding.ASCII.GetBytes(html), 0, html.Length);
            outputStream.Length.ShouldEqual(html.Length);
        }

        [Fact]
        public void Html_has_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("text/html");

            filter.Write(Encoding.ASCII.GetBytes("<html/>"), 0, 7);

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html/>"));
        }

        [Fact]
        public void XHtml_has_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("application/xhtml+xml");

            filter.Write(Encoding.ASCII.GetBytes("<html/>"), 0, 7);

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html/>"));
        }

        [Fact]
        public void Non_html_output_does_not_have_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("text/plain");
            
            filter.Write(Encoding.ASCII.GetBytes("test"), 0, 4);

            outputStream.GetBuffer().SequenceEqual(Encoding.ASCII.GetBytes("test"));
        }
    }
}
