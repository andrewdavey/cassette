using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class PlaceholderReplacingResponseFilter_Tests : IDisposable
    {
        readonly MemoryStream outputStream;
        readonly Mock<HttpResponseBase> response;
        readonly Mock<HttpRequestBase> request;
        readonly Mock<IPlaceholderTracker> placeholderTracker;
        readonly PlaceholderReplacingResponseFilter filter;

        public PlaceholderReplacingResponseFilter_Tests()
        {
            outputStream = new MemoryStream();
            response = new Mock<HttpResponseBase>();
            request = new Mock<HttpRequestBase>();
            request.SetupGet(r => r.Headers).Returns(new NameValueCollection());
            
            
            placeholderTracker = new Mock<IPlaceholderTracker>();

            placeholderTracker.Setup(h => h.ReplacePlaceholders(It.IsAny<string>()))
                      .Returns<string>(s => s);

            response.SetupGet(r => r.Output.Encoding).Returns(Encoding.ASCII);
            response.SetupGet(r => r.Filter).Returns(outputStream);
            response.SetupGet(r => r.Headers).Returns(new NameValueCollection());
            filter = new PlaceholderReplacingResponseFilter(new CassetteSettings(),  response.Object, request.Object, placeholderTracker.Object);
        }

        [Fact]
        public void WhenWriteAndClose_ThenDataWrittenToOutputStream()
        {
            Write("<html></html>");
            Close();

            Encoding.ASCII.GetString(outputStream.ToArray()).ShouldEqual("<html></html>");
        }

        [Fact]
        public void WhenWriteAndClose_ThenReplacePlaceholdersIsCalled()
        {
            Write("<html></html>");
            Close();

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html></html>"));
        }

        [Fact]
        public void WhenTwoWriteCallsAndClose_ThenAllOutputIsWrittenToOutputStream()
        {
            Write("<html>start");
            Write("end</html>");
            Close();

            Encoding.ASCII.GetString(outputStream.ToArray()).ShouldEqual("<html>startend</html>");
        }

        [Fact]
        public void WhenTwoWriteCalls_ThenReplacePlaceholdersIsCalledWithAllOutput()
        {
            Write("<html>start");
            Write("end</html>");
            Close();

            placeholderTracker.Verify(t => t.ReplacePlaceholders("<html>startend</html>"));
        }

        [Fact]
        public void WhenWriteAndNoClose_ThenNoOutputWrittenToStream()
        {
            Write("<html>start</html>");

            Encoding.ASCII.GetString(outputStream.ToArray()).ShouldEqual("");
        }

        void Write(string content)
        {
            filter.Write(Encoding.ASCII.GetBytes(content), 0, content.Length);
        }

        void Close()
        {
            filter.Close();
        }

        void IDisposable.Dispose()
        {
            filter.Dispose();
            outputStream.Dispose();
        }
    }
}