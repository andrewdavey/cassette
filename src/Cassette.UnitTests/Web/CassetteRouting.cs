using System.Linq;
using System.Web;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public abstract class CassetteRouting_Tests
    {
        protected readonly Mock<IUrlModifier> urlModifier = new Mock<IUrlModifier>();
        internal readonly CassetteRouting routing;

        public CassetteRouting_Tests()
        {
            urlModifier.Setup(m => m.Modify(It.IsAny<string>()))
                       .Returns<string>(url => url);

            var container = new Mock<ICassetteApplicationContainer<ICassetteApplication>>();
            container.SetupGet(c => c.Application.Bundles).Returns(Enumerable.Empty<Bundle>());
            routing = new CassetteRouting(container.Object, "_cassette");
        }
    }

    public class UrlGenerator_InstallRoutes_Tests : CassetteRouting_Tests
    {
        readonly RouteCollection routes;
        readonly Mock<HttpContextBase> httpContext;

        public UrlGenerator_InstallRoutes_Tests()
        {
            routes = new RouteCollection();
            routing.InstallRoutes(routes);
            httpContext = new Mock<HttpContextBase>();
        }

        void SetupAppRelativeCurrentExecutionFilePath(string path)
        {
            httpContext.Setup(c => c.Request.AppRelativeCurrentExecutionFilePath)
                       .Returns(path);
        }

        [Fact]
        public void ScriptBundleUrlMappedToScriptBundleHandler()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/scriptbundle/test_ABC");

            var routeData = routes.GetRouteData(httpContext.Object);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(httpContext.Object, routeData));
            httpHandler.ShouldBeType<BundleRequestHandler<ScriptBundle>>();
        }

        [Fact]
        public void ScriptBundleUrlAssignsPathRouteDataValue()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/scriptbundle/test_ABC");

            var routeData = routes.GetRouteData(httpContext.Object);

            routeData.Values["path"].ShouldEqual("test_ABC");
        }

        [Fact]
        public void ScriptBundleUrlWithDirectoryAssignsPathRouteDataValue()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/scriptbundle/scripts/test_ABC");

            var routeData = routes.GetRouteData(httpContext.Object);

            routeData.Values["path"].ShouldEqual("scripts/test_ABC");
        }

        [Fact]
        public void StylesheetBundleUrlMappedToStylesheetBundleHandler()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/stylesheetbundle/test_ABC");

            var routeData = routes.GetRouteData(httpContext.Object);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(httpContext.Object, routeData));
            httpHandler.ShouldBeType<BundleRequestHandler<StylesheetBundle>>();
        }

        [Fact]
        public void HtmlTemplateBundleUrlMappedToHtmlTemplateBundleHandler()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/htmltemplatebundle/test_ABC_htmltemplate");

            var routeData = routes.GetRouteData(httpContext.Object);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(httpContext.Object, routeData));
            httpHandler.ShouldBeType<BundleRequestHandler<HtmlTemplateBundle>>();
        }

        [Fact]
        public void AssetUrlIsMappedToAssetHandler()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/asset/test_coffee");

            var routeData = routes.GetRouteData(httpContext.Object);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(httpContext.Object, routeData));
            httpHandler.ShouldBeType<AssetRequestHandler>();
        }

        [Fact]
        public void RawFileUrlIsMappedToRawFileHandler()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/file/test_coffee");

            var routeData = routes.GetRouteData(httpContext.Object);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(httpContext.Object, routeData));
            httpHandler.ShouldBeType<RawFileRequestHandler>();
        }
    }
}