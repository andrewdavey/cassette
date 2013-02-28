﻿using System;
using System.IO;
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

        public CachedFileRequestHandler_ProcessRequestTests()
        {
            cacheDirectory = new FakeFileSystem();

            response = new Mock<HttpResponseBase>();
            outputStream = new MemoryStream();
            response.SetupGet(r => r.OutputStream).Returns(outputStream);
            cache = new Mock<HttpCachePolicyBase>();
            response.SetupGet(r => r.Cache).Returns(cache.Object);

            handler = new CachedFileRequestHandler(response.Object, cacheDirectory);
        }

        [Fact]
        public void WritesCacheFileToOutputStream()
        {
            cacheDirectory.Add("~/file.png", new byte[] { 1, 2, 3 });
            handler.ProcessRequest("~/file.png");
            outputStream.ToArray().ShouldEqual(new byte[] { 1, 2, 3 });
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