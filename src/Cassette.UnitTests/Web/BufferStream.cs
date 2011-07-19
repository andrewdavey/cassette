using System.IO;
using System.Text;
using System.Web;
using Moq;
using Should;
using Xunit;
using System.Linq;

namespace Cassette.Web
{
    public class BufferStream_tests
    {
        MemoryStream outputStream;
        Mock<HttpContextBase> context;
        Mock<IPageHelper> pageHelper;
        BufferStream buffer;

        public BufferStream_tests()
        {
            outputStream = new MemoryStream();
            context = new Mock<HttpContextBase>();
            pageHelper = new Mock<IPageHelper>();

            pageHelper.Setup(h => h.ReplacePlaceholders(It.IsAny<string>()))
                      .Returns<string>(s => s);

            context.SetupGet(c => c.Response.ContentType).Returns("text/html");
            context.SetupGet(c => c.Response.Output.Encoding).Returns(Encoding.ASCII);

            buffer = new BufferStream(outputStream, context.Object, pageHelper.Object);
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
            context.SetupGet(c => c.Response.ContentType).Returns("application/xhtml+xml");

            buffer.Write(Encoding.ASCII.GetBytes("<html/>"), 0, 7);
            buffer.Flush();

            pageHelper.VerifyAll();
        }

        [Fact]
        public void Non_html_output_does_not_have_placeholders_replaced()
        {
            context.SetupGet(c => c.Response.ContentType).Returns("text/plain");
            
            buffer.Write(Encoding.ASCII.GetBytes("test"), 0, 4);
            buffer.Flush();

            outputStream.GetBuffer().SequenceEqual(Encoding.ASCII.GetBytes("test"));
        }
    }
}
