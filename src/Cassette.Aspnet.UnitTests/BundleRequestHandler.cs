using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class BundleRequestHandler_Tests : IDisposable
    {
        readonly Mock<HttpContextBase> httpContext;
        protected readonly Mock<HttpRequestBase> request;
        protected readonly Mock<HttpResponseBase> response;
        protected readonly Mock<HttpCachePolicyBase> responseCache;
        protected readonly NameValueCollection requestHeaders;
        readonly RouteData routeData;
        readonly RequestContext requestContext;
        protected readonly Stream outputStream;
        protected readonly BundleCollection bundles;

        protected BundleRequestHandler_Tests()
        {
            httpContext = new Mock<HttpContextBase>();
            request = new Mock<HttpRequestBase>();
            requestHeaders = new NameValueCollection();
            response = new Mock<HttpResponseBase>();
            responseCache = new Mock<HttpCachePolicyBase>();
            routeData = new RouteData();
            requestContext = new RequestContext(httpContext.Object, routeData);
            outputStream = new MemoryStream();

            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            httpContext.SetupGet(c => c.Items).Returns(new Dictionary<string, object>());

            response.SetupGet(r => r.OutputStream).Returns(outputStream);
            response.SetupGet(r => r.Cache).Returns(responseCache.Object);

            request.SetupGet(r => r.Headers).Returns(requestHeaders);
            request.SetupGet(x => x.RawUrl).Returns("~/010203/test");

            var settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>(), Mock.Of<IBundleCollectionInitializer>());
        }

        internal BundleRequestHandler<TestableBundle> CreateRequestHandler()
        {
            return new BundleRequestHandler<TestableBundle>(
                bundles,
                requestContext
            );
        }

        protected void SetupTestBundle()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new StubAsset("~/asset.js", "asset-content");
            bundle.Assets.Add(asset);
            bundle.Hash = new byte[] { 1, 2, 3 };
            bundles.Add(bundle);
            bundles.BuildReferences();
            bundle.Process(new CassetteSettings());
        }

        void IDisposable.Dispose()
        {
            outputStream.Dispose();
        }
    }

    public class GivenBundleExists_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenBundleExists_WhenProcessRequest()
        {
            var bundle = new TestableBundle("~/test") { ContentType = "expected-content-type" };
            var asset = new StubAsset("~/asset.js", "asset-content");
            bundle.Hash = new byte[] { 1, 2, 3 };
            bundle.Assets.Add(asset);
            bundles.Add(bundle);
            bundles.BuildReferences();
            bundle.Process(new CassetteSettings());

            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        readonly DateTime start = DateTime.UtcNow;

        [Fact]
        public void BundleAssetContentReturned()
        {
            outputStream.Position = 0;
            outputStream.ReadToEnd().ShouldEqual("asset-content");
        }

        [Fact]
        public void ContentTypeIsTheBundleContentType()
        {
            response.VerifySet(r => r.ContentType = "expected-content-type");
        }

        [Fact]
        public void ExpiresInOneYear()
        {
            responseCache.Verify(c => c.SetExpires(It.Is<DateTime>(d => (d - start).TotalDays >= 365)));
        }

        [Fact]
        public void MaxAgeIsSetToOneYear()
        {
            // Setting the cache to Public, then the MaxAge will be set on the asset.
            responseCache.Verify(c => c.SetCacheability(HttpCacheability.Public));
            responseCache.Verify(c => c.SetMaxAge(It.Is<TimeSpan>(t => t.TotalDays >= 365)));
        }

        [Fact]
        public void ResponseIsPubliclyCacheable()
        {
            responseCache.Verify(c => c.SetCacheability(HttpCacheability.Public));
        }

        [Fact]
        public void ETagIsAssetHash()
        {
            var expectedETag = "\"010203\"";
            responseCache.Verify(c => c.SetETag(expectedETag));
        }
    }

    public class GivenBundleExists_WhenProcessRequest_With_Hash_Mismatch : BundleRequestHandler_Tests {
        public GivenBundleExists_WhenProcessRequest_With_Hash_Mismatch()
        {
            SetupTestBundle();
            request.SetupGet(x => x.RawUrl).Returns("~/HASH-MISMATCH/test");
            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void ResponseSetToNoCache() {
            response.VerifySet(r => r.CacheControl = "no-cache");
        }
    }


    public class GivenBundleDoesNotExist : BundleRequestHandler_Tests
    {
        [Fact]
        public void HandlerReturns404()
        {
            var handler = CreateRequestHandler();
            var httpException = Assert.Throws<HttpException>(() => handler.ProcessRequest("~/notfound"));
            httpException.GetHttpCode().ShouldEqual(404);
            response.VerifySet(r => r.StatusCode = 404);
        }
    }

    public class GivenBundleExistsAndIfNonMatchHeaderIsEqualAssetHash_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenBundleExistsAndIfNonMatchHeaderIsEqualAssetHash_WhenProcessRequest()
        {
            SetupTestBundle();

            requestHeaders["If-None-Match"] = "\"010203\"";
            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void StatusCodeIs304NotModified()
        {
            response.VerifySet(r => r.StatusCode = 304);
        }
    }

    public class GivenBundleExistsAndIfNonMatchHeaderIsNotEqualAssetHash_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenBundleExistsAndIfNonMatchHeaderIsNotEqualAssetHash_WhenProcessRequest()
        {
            SetupTestBundle();

            requestHeaders["If-None-Match"] = "xxxxxx";
            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void BundleAssetContentReturned()
        {
            outputStream.Position = 0;
            (outputStream.Length > 0).ShouldBeTrue();
        }
    }

    public class GivenRequestDeflateEncoding_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenRequestDeflateEncoding_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "deflate");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestBundle();

            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void ResponseFilterIsDeflateStream()
        {
            response.VerifySet(r => r.Filter = It.IsAny<DeflateStream>());
        }

        [Fact]
        public void ContentEncodingHeaderIsDeflate()
        {
            response.Verify(r => r.AppendHeader("Content-Encoding", "deflate"));
        }

        [Fact]
        public void VeryHeaderIsAcceptEncoding()
        {
            response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }
    }

    public class GivenRequestGZipEncoding_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenRequestGZipEncoding_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "gzip");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestBundle();

            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void ResponseFilterIsGZipStream()
        {
            response.VerifySet(r => r.Filter = It.IsAny<GZipStream>());
        }

        [Fact]
        public void ContentEncodingHeaderIsGzip()
        {
            response.Verify(r => r.AppendHeader("Content-Encoding", "gzip"));
        }

        [Fact]
        public void VeryHeaderIsAcceptEncoding()
        {
            response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }
    }

    public class GivenRequestMultipleEncodingsNoQValue_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenRequestMultipleEncodingsNoQValue_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "gzip,deflate");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestBundle();

            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void ResponseFilterIsGZipStream()
        {
            response.VerifySet(r => r.Filter = It.IsAny<GZipStream>());
        }

        [Fact]
        public void ContentEncodingHeaderIsGzip()
        {
            response.Verify(r => r.AppendHeader("Content-Encoding", "gzip"));
        }

        [Fact]
        public void VeryHeaderIsAcceptEncoding()
        {
            response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }
    }

    public class GivenRequestMultipleEncodingsEquivalentQValues_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenRequestMultipleEncodingsEquivalentQValues_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "gzip;q=1.0, deflate;q=1.0");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestBundle();

            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void ResponseFilterIsGZipStream()
        {
            response.VerifySet(r => r.Filter = It.IsAny<GZipStream>());
        }

        [Fact]
        public void ContentEncodingHeaderIsGzip()
        {
            response.Verify(r => r.AppendHeader("Content-Encoding", "gzip"));
        }

        [Fact]
        public void VeryHeaderIsAcceptEncoding()
        {
            response.Verify(r => r.AppendHeader("Vary", "Accept-Encoding"));
        }
    }

    public class GivenRequestWithUnrecognizedEncoding_WhenProcessRequest : BundleRequestHandler_Tests
    {
        public GivenRequestWithUnrecognizedEncoding_WhenProcessRequest()
        {
            requestHeaders.Add("Accept-Encoding", "unknown");
            response.SetupGet(r => r.Filter).Returns(Stream.Null);

            SetupTestBundle();

            var handler = CreateRequestHandler();
            handler.ProcessRequest("~/test");
        }

        [Fact]
        public void ResponseFilterIsNotSet()
        {
            response.VerifySet(r => r.Filter = It.IsAny<Stream>(), Times.Once()); // Only set once in the test constructor.
        }
    }
}