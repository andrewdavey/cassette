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
            response = new Mock<HttpResponseBase>();

            routeData.Values.Add("path", "test/asset.js");
            requestContext.SetupGet(r => r.RouteData)
                          .Returns(routeData);

            requestContext.SetupGet(r => r.HttpContext.Response)
                          .Returns(response.Object);

            response.SetupGet(r => r.OutputStream).Returns(() => outputStream);

            handler = new AssetRequestHandler(requestContext.Object, _ => module);
        }

        readonly AssetRequestHandler handler;
        readonly Mock<HttpResponseBase> response;
        Module module;
        MemoryStream outputStream;

        [Fact]
        public void IsReusableIsFalse()
        {
            handler.IsReusable.ShouldBeFalse();
        }

        [Fact]
        public void GivenModuleIsNull_WhenProcessRequest_ThenNotFoundResponse()
        {
            handler.ProcessRequest(null);
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void GivenModuleFoundButAssetIsNull_WhenProcessRequest_ThenNotFoundResponse()
        {
            module = new Module("~/test");
            handler.ProcessRequest(null);
            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void GivenAssetExists_WhenProcessRequest_ThenResponseContentTypeIsModuleContentType()
        {
            module = new Module("~/test")
            {
                ContentType = "CONTENT/TYPE"
            };
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename)
                 .Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream())
                 .Returns(Stream.Null);
            module.Assets.Add(asset.Object);

            using (outputStream = new MemoryStream())
            {
                handler.ProcessRequest(null);
            }

            response.VerifySet(r => r.ContentType = "CONTENT/TYPE");
        }

        [Fact]
        public void GivenAssetExists_WhenProcessRequest_ThenResponseOutputIsAssetContent()
        {
            module = new Module("~/test")
            {
                ContentType = "CONTENT/TYPE"
            };
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename)
                 .Returns("~/test/asset.js");
            asset.Setup(a => a.OpenStream())
                 .Returns(() => "output".AsStream());
            module.Assets.Add(asset.Object);

            using (outputStream = new MemoryStream())
            {
                handler.ProcessRequest(null);

                Encoding.UTF8.GetString(outputStream.ToArray()).ShouldEqual("output");
            }
        }
    }
}