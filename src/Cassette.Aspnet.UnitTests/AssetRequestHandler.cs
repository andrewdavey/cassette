using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class AssetRequestHandler_Tests
    {
        public AssetRequestHandler_Tests()
        {            
            var routeData = new RouteData();            
            request = new Mock<HttpRequestBase>();
            response = new Mock<HttpResponseBase>();
            cache = new Mock<HttpCachePolicyBase>();
            requestHeaders = new NameValueCollection();

            routeData.Values.Add("path", "test/asset_js");

            var httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(r => r.Response)
                          .Returns(response.Object);
            httpContext.SetupGet(r => r.Request)
                          .Returns(request.Object);
            httpContext.SetupGet(r => r.Items)
                          .Returns(new Dictionary<string, object>());

            var requestContext = new RequestContext(httpContext.Object, routeData);

            response.SetupGet(r => r.OutputStream).Returns(() => outputStream);
            response.SetupGet(r => r.Cache).Returns(cache.Object);
            request.SetupGet(r => r.Headers).Returns(requestHeaders);

            bundles = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            handler = new AssetRequestHandler(requestContext, bundles);
        }

        readonly AssetRequestHandler handler;
        readonly Mock<HttpRequestBase> request;
        readonly Mock<HttpResponseBase> response;
        readonly Mock<HttpCachePolicyBase> cache;
        readonly NameValueCollection requestHeaders;
        readonly BundleCollection bundles;
        MemoryStream outputStream;

        [Fact]
        public void GivenBundleNotFound_WhenProcessRequest_ThenNotFoundResponse()
        {
            bundles.Clear();
            handler.ProcessRequest(null);
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void GivenBundleFoundButAssetIsNull_WhenProcessRequest_ThenNotFoundResponse()
        {
            bundles.Add(new TestableBundle("~/test"));
            handler.ProcessRequest("~/test/asset.js");
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void GivenAssetExists_WhenProcessRequest_ThenResponseContentTypeIsBundleContentType()
        {
            bundles.Add(new TestableBundle("~/test")
            {
                ContentType = "CONTENT/TYPE"
            });
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.Path)
                 .Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream())
                 .Returns(Stream.Null);
            bundles.First().Assets.Add(asset.Object);

            using (outputStream = new MemoryStream())
            {
                handler.ProcessRequest("~/test/asset.js");
            }

            response.VerifySet(r => r.ContentType = "CONTENT/TYPE");
        }

        [Fact]
        public void GivenAssetExists_WhenProcessRequest_ThenResponseOutputIsAssetContent()
        {
            bundles.Add(new TestableBundle("~/test")
            {
                ContentType = "CONTENT/TYPE"
            });
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.Path)
                 .Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream())
                 .Returns(() => "output".AsStream());
            bundles.First().Assets.Add(asset.Object);

            using (outputStream = new MemoryStream())
            {
                handler.ProcessRequest("~/test/asset.js");

                Encoding.UTF8.GetString(outputStream.ToArray()).ShouldEqual("output");
            }
        }

        [Fact]
        public void GivenRequestWithMatchingETag_WhenProcessRequest_ThenReturn304NotModifiedResponse()
        {
            bundles.Add(new TestableBundle("~/test"));
            var asset = new StubAsset("~/test/asset.js", hash: new byte[] { 1, 2, 3 });
            bundles.First().Assets.Add(asset);

            using (outputStream = new MemoryStream())
            {
                requestHeaders.Add("If-None-Match", "\"010203\"");
                handler.ProcessRequest("~/test/asset.js");
            }

            response.VerifySet(r => r.StatusCode = 304);
        }
    }
}