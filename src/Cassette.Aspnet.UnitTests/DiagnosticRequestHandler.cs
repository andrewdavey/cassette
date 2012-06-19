using System.Web;
using System.Web.Routing;
using Cassette.Views;
using Xunit;
using Moq;
using Cassette.Scripts;

namespace Cassette.Aspnet
{
    public class DiagnosticRequestHandler_Tests
    {
        readonly DiagnosticRequestHandler handler;
        readonly Mock<HttpContextBase> httpContext;
        readonly CassetteSettings settings;
        readonly Mock<HttpResponseBase> response;

        public DiagnosticRequestHandler_Tests()
        {
            httpContext = new Mock<HttpContextBase>();
            response = new Mock<HttpResponseBase>();
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            var requestContext = new RequestContext(httpContext.Object, new RouteData());
            settings = new CassetteSettings();
            var bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var rebuilder = Mock.Of<IBundleCacheRebuilder>();
            var helper = new Mock<IBundlesHelper>();
            helper.Setup(h => h.Render<ScriptBundle>(null)).Returns(new HtmlString(""));
            Bundles.Helper = helper.Object;
            httpContext.SetupGet(c => c.Request.HttpMethod).Returns("GET");
            handler = new DiagnosticRequestHandler(requestContext, bundles, settings, urlGenerator, rebuilder);
        }

        [Fact]
        public void GivenNotAllowRemoteDiagnostics_WhenProcessRemoteRequest_ThenReturn404()
        {
            httpContext.Setup(c => c.Request.IsLocal).Returns(false);
            settings.AllowRemoteDiagnostics = false;

            handler.ProcessRequest();

            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void WhenProcessLocalRequest_ThenReturnHtml()
        {
            httpContext.Setup(c => c.Request.IsLocal).Returns(true);

            handler.ProcessRequest();

            response.Verify(r => r.Write(It.Is<string>(html => html.StartsWith("<!DOCTYPE html>"))));
        }
    }
}