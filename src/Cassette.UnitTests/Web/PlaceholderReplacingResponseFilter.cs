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
        PlaceholderReplacingResponseFilter buffer;

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
            buffer = new PlaceholderReplacingResponseFilter(response.Object, placeholderTracker.Object);
        }

        [Fact]
        public void Write_is_buffered_until_Flush()
        {
            var html = "<html>\r\n";
            buffer.Write(Encoding.ASCII.GetBytes(html), 0, html.Length);
            outputStream.Length.ShouldEqual(0);

            buffer.Flush();
            outputStream.Length.ShouldEqual(html.Length);
        }

        [Fact]
        public void XHtml_has_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("application/xhtml+xml");

            buffer.Write(Encoding.ASCII.GetBytes("<html/>"), 0, 7);
            buffer.Flush();

            placeholderTracker.VerifyAll();
        }

        [Fact]
        public void Non_html_output_does_not_have_placeholders_replaced()
        {
            response.SetupGet(r => r.ContentType).Returns("text/plain");
            
            buffer.Write(Encoding.ASCII.GetBytes("test"), 0, 4);
            buffer.Flush();

            outputStream.GetBuffer().SequenceEqual(Encoding.ASCII.GetBytes("test"));
        }
    }
}
