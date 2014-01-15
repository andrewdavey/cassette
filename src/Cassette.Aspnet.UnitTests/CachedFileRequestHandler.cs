using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class CachedFileRequestHandler_ProcessRequestTests
    {
        readonly CachedFileRequestHandler handler;
        readonly MemoryStream outputStream;
        readonly Mock<HttpResponseBase> response;
        readonly FakeFileSystem cacheDirectory;
        readonly Mock<HttpCachePolicyBase> cache;
		readonly Mock<HttpRequestBase> request;
		readonly NameValueCollection requestHeaders;

        public CachedFileRequestHandler_ProcessRequestTests()
        {
			cacheDirectory = new FakeFileSystem();
			request = new Mock<HttpRequestBase>();
			requestHeaders = new NameValueCollection();
			
            response = new Mock<HttpResponseBase>();
            outputStream = new MemoryStream();
            response.SetupGet(r => r.OutputStream).Returns(outputStream);
            cache = new Mock<HttpCachePolicyBase>();
			response.SetupGet(r => r.Cache).Returns(cache.Object);
			request.SetupGet(r => r.Headers).Returns(requestHeaders);

            handler = new CachedFileRequestHandler(request.Object, response.Object, cacheDirectory);
        }

        [Fact]
        public void WritesCacheFileToOutputStream()
        {
            cacheDirectory.Add("~/file.png", new byte[] { 1, 2, 3 });
            handler.ProcessRequest("~/file.png");
            outputStream.ToArray().ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void GivenRequestContainsEncodingGZip_WritesCacheFileAsEncodedGZip()
        {
			cacheDirectory.Add("~/file.png", new byte[] { 1, 2, 3 });
			requestHeaders.Add("Accept-Encoding", "gzip");
			response.SetupGet(r => r.Filter).Returns(Stream.Null);

            handler.ProcessRequest("~/file.png");

			response.VerifySet(r => r.Filter = It.IsAny<GZipStream>());
			response.Verify(r => r.AppendHeader("Content-Encoding", "gzip"));
			response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }

        [Fact]
		public void GivenRequestContainsEncodingDeflate_WritesCacheFileAsEncodedDeflate()
        {
			cacheDirectory.Add("~/file.png", new byte[] { 1, 2, 3 });
			requestHeaders.Add("Accept-Encoding", "deflate");
			response.SetupGet(r => r.Filter).Returns(Stream.Null);

            handler.ProcessRequest("~/file.png");

			response.VerifySet(r => r.Filter = It.IsAny<DeflateStream>());
			response.Verify(r => r.AppendHeader("Content-Encoding", "deflate"));
			response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }

        [Fact]
        public void StatusCode404WhenCacheFileDoesntExist()
        {
            var httpException = Assert.Throws<HttpException>(() => handler.ProcessRequest("~/notfound"));
            httpException.GetHttpCode().ShouldEqual(404);
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void ExpiresInTheFuture()
        {
            cacheDirectory.Add("~/file.png", new byte[] { 1, 2, 3 });
            handler.ProcessRequest("~/file.png");
            cache.Verify(c => c.SetExpires(It.Is<DateTime>(d => d > DateTime.UtcNow)));
        }
    }
}