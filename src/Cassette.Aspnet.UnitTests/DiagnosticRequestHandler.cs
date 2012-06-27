using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Views;
using Moq;
using Should;
using Xunit;

namespace Cassette.Aspnet
{
    public class DiagnosticRequestHandler_Tests
    {
        readonly DiagnosticRequestHandler handler;
        readonly Mock<HttpContextBase> httpContext;
        readonly CassetteSettings settings;
        readonly Mock<HttpResponseBase> response;
        readonly Mock<IBundleCacheRebuilder> rebuilder;
        readonly BundleCollection bundles;
        readonly Mock<IBundlesHelper> helper;

        public DiagnosticRequestHandler_Tests()
        {
            httpContext = new Mock<HttpContextBase>();
            response = new Mock<HttpResponseBase>();
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            var requestContext = new RequestContext(httpContext.Object, new RouteData());
            settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            var urlGenerator = Mock.Of<IUrlGenerator>();
            rebuilder = new Mock<IBundleCacheRebuilder>();
            helper = new Mock<IBundlesHelper>();
            helper.Setup(h => h.Render<ScriptBundle>(null)).Returns(new HtmlString(""));
            Bundles.Helper = helper.Object;
            httpContext.SetupGet(c => c.Request.HttpMethod).Returns("GET");
            handler = new DiagnosticRequestHandler(requestContext, bundles, settings, urlGenerator, rebuilder.Object);
        }

        [Fact]
        public void GivenNotAllowRemoteDiagnostics_WhenProcessRemoteRequest_ThenReturn404()
        {
            GivenRemoteRequest();
            settings.AllowRemoteDiagnostics = false;

            handler.ProcessRequest();

            response.VerifySet(r => r.StatusCode = 404);
        }

        [Fact]
        public void WhenProcessLocalRequest_ThenReturnHtml()
        {
            GivenLocalRequest();

            handler.ProcessRequest();

            response.Verify(r => r.Write(It.Is<string>(html => html.StartsWith("<!DOCTYPE html>"))));
        }

        [Fact]
        public void GivenScriptBundleInCollection_WhenProcessRequest_ThenPageDataScriptIncludesBundleJson()
        {
            GivenLocalRequest();
            bundles.Add(new ScriptBundle("~/test"));
            PageDataScriptBundle bundle = null;
            helper
                .Setup(h => h.Reference(It.IsAny<PageDataScriptBundle>(), null))
                .Callback<Bundle, string>((referenced, _) => bundle = (PageDataScriptBundle)referenced);

            handler.ProcessRequest();

            var script = bundle.Render();
            script.ShouldContain("Scripts=[{\"Path\":\"~/test\",\"Url\":null,\"Condition\":null,\"Assets\":[],\"References\":[],\"Size\":-1}];");
        }

        [Fact]
        public void GivenStylesheetBundleInCollection_WhenProcessRequest_ThenPageDataScriptIncludesBundleJson()
        {
            GivenLocalRequest();
            bundles.Add(new StylesheetBundle("~/test"));
            PageDataScriptBundle bundle = null;
            helper
                .Setup(h => h.Reference(It.IsAny<PageDataScriptBundle>(), null))
                .Callback<Bundle, string>((referenced, _) => bundle = (PageDataScriptBundle)referenced);

            handler.ProcessRequest();

            var script = bundle.Render();
            script.ShouldContain("Stylesheets=[{\"Path\":\"~/test\",\"Url\":null,\"Media\":null,\"Condition\":null,\"Assets\":[],\"References\":[],\"Size\":-1}];");
        }

        [Fact]
        public void GivenHtmlTemplateBundleInCollection_WhenProcessRequest_ThenPageDataScriptIncludesBundleJson()
        {
            GivenLocalRequest();
            bundles.Add(new HtmlTemplateBundle("~/test"));
            PageDataScriptBundle bundle = null;
            helper
                .Setup(h => h.Reference(It.IsAny<PageDataScriptBundle>(), null))
                .Callback<Bundle, string>((referenced, _) => bundle = (PageDataScriptBundle)referenced);

            handler.ProcessRequest();

            var script = bundle.Render();
            script.ShouldContain("HtmlTemplates=[{\"Path\":\"~/test\",\"Url\":null,\"Assets\":[],\"References\":[],\"Size\":-1}];");
        }

        [Fact]
        public void WhenPostActionRebuildCache_ThenCacheIsRebuilt()
        {
            GivenLocalRequest();
            GivenPostRequest();
            GivenForm(new NameValueCollection
            {
                { "action", "rebuild-cache" }
            });

            handler.ProcessRequest();

            rebuilder.Verify(r => r.RebuildCache());
        }

        void GivenPostRequest()
        {
            httpContext.SetupGet(c => c.Request.HttpMethod).Returns("POST");
        }

        void GivenRemoteRequest()
        {
            httpContext.Setup(c => c.Request.IsLocal).Returns(false);
        }

        void GivenLocalRequest()
        {
            httpContext.Setup(c => c.Request.IsLocal).Returns(true);
        }

        void GivenForm(NameValueCollection form)
        {
            httpContext.SetupGet(c => c.Request.Form).Returns(form);
        }
    }
}