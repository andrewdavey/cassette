using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Routing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class AssetRequestHandler_Tests
    {
        public AssetRequestHandler_Tests()
        {
            var requestContext = new Mock<RequestContext>();
            var routeData = new RouteData();
            request = new Mock<HttpRequestBase>();
            response = new Mock<HttpResponseBase>();
            cache = new Mock<HttpCachePolicyBase>();
            requestHeaders = new NameValueCollection();

            routeData.Values.Add("path", "test/asset.js");
            requestContext.SetupGet(r => r.RouteData)
                          .Returns(routeData);

            requestContext.SetupGet(r => r.HttpContext.Response)
                          .Returns(response.Object);
            requestContext.SetupGet(r => r.HttpContext.Request)
                          .Returns(request.Object);
            requestContext.SetupGet(r => r.HttpContext.Items)
                          .Returns(new Dictionary<string, object>());

            response.SetupGet(r => r.OutputStream).Returns(() => outputStream);
            response.SetupGet(r => r.Cache).Returns(cache.Object);
            request.SetupGet(r => r.Headers).Returns(requestHeaders);

            bundleContainer = new Mock<IBundleContainer>();
            bundleContainer.Setup(c => c.FindBundlesContainingPath(It.IsAny<string>()))
                           .Returns(() => new[] { bundle });
            handler = new AssetRequestHandler(requestContext.Object, () => bundleContainer.Object);
        }

        readonly AssetRequestHandler handler;
        readonly Mock<HttpRequestBase> request;
        readonly Mock<HttpResponseBase> response;
        readonly Mock<HttpCachePolicyBase> cache;
        readonly NameValueCollection requestHeaders;
        readonly Mock<IBundleContainer> bundleContainer;
        Bundle bundle;
        MemoryStream outputStream;

        [Fact]
        public void IsReusableIsFalse()
        {
            handler.IsReusable.ShouldBeFalse();
        }

        [Fact]
        public void GivenBundleNotFound_WhenProcessRequest_ThenNotFoundResponse()
        {
            bundleContainer.Setup(c => c.FindBundlesContainingPath(It.IsAny<string>()))
                           .Returns(() => new Bundle[0]);
            handler.ProcessRequest(null);
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void GivenBundleFoundButAssetIsNull_WhenProcessRequest_ThenNotFoundResponse()
        {
            bundle = new TestableBundle("~/test");
            handler.ProcessRequest(null);
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void GivenAssetExists_WhenProcessRequest_ThenResponseContentTypeIsBundleContentType()
        {
            bundle = new TestableBundle("~/test")
            {
                ContentType = "CONTENT/TYPE"
            };
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFile.FullPath)
                 .Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream())
                 .Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);

            using (outputStream = new MemoryStream())
            {
                handler.ProcessRequest(null);
            }

            response.VerifySet(r => r.ContentType = "CONTENT/TYPE");
        }

        [Fact]
        public void GivenAssetExists_WhenProcessRequest_ThenResponseOutputIsAssetContent()
        {
            bundle = new TestableBundle("~/test")
            {
                ContentType = "CONTENT/TYPE"
            };
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFile.FullPath)
                 .Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream())
                 .Returns(() => "output".AsStream());
            bundle.Assets.Add(asset.Object);

            using (outputStream = new MemoryStream())
            {
                handler.ProcessRequest(null);

                Encoding.UTF8.GetString(outputStream.ToArray()).ShouldEqual("output");
            }
        }
    }
}
