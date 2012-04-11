using System.Web;
using System.Web.Routing;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Web
{
    public abstract class RouteInstaller_Tests
    {
        readonly RouteInstaller routing;
        readonly RouteCollection routes = new RouteCollection();
        readonly Mock<HttpContextBase> httpContext;

        protected RouteInstaller_Tests()
        {
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var settings = new CassetteSettings();
            var bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            httpContext = new Mock<HttpContextBase>();
            var container = new TinyIoCContainer();
            container.Register(bundles);
            container.Register(settings);
            container.Register(typeof(IUrlGenerator), urlGenerator);

            routing = new RouteInstaller(routes, container);

            routing.Start();
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