using System;
using System.Web;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;
using System.Linq;

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
            routing = new CassetteRouting(urlModifier.Object, container.Object);
        }
    }

    public class CassetteRouting_CreateBundleUrl_Tests : CassetteRouting_Tests
    {
        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            routing.CreateBundleUrl(StubScriptBundle("~/test"));
            urlModifier.Verify(m => m.Modify("_cassette/scriptbundle/test_010203"));
        }

        [Fact]
        public void CreateScriptBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndPathAndHash()
        {
            var url = routing.CreateBundleUrl(StubScriptBundle("~/test/foo"));
            url.ShouldEqual("_cassette/scriptbundle/test/foo_010203");
        }

        [Fact]
        public void CreateStylesheetBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndPathAndHash()
        {
            var url = routing.CreateBundleUrl(StubStylesheetBundle("~/test/foo"));
            url.ShouldEqual("_cassette/stylesheetbundle/test/foo_010203");
        }

        static ScriptBundle StubScriptBundle(string path)
        {
            var bundle = new ScriptBundle(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            bundle.Assets.Add(asset.Object);
            return bundle;
        }

        static StylesheetBundle StubStylesheetBundle(string path)
        {
            var bundle = new StylesheetBundle(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            bundle.Assets.Add(asset.Object);
            return bundle;
        }
    }

    public class UrlGenerator_CreateAssetUrl_Tests : CassetteRouting_Tests
    {
        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[0]);

            routing.CreateAssetUrl(asset.Object);

            urlModifier.Verify(m => m.Modify(It.IsAny<string>()));
        }

        [Fact]
        public void CreateAssetUrlReturnsCompileUrl()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            
            var url = routing.CreateAssetUrl(asset.Object);

            url.ShouldEqual("_cassette/asset/test/asset.coffee?01020f10");
        }
    }

    public class UrlGenerator_CreateImageUrl_Tests : CassetteRouting_Tests
    {
        [Fact]
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndPathWithoutTildeAndHashAndExtensionDotConvertedToUnderscore()
        {
            var url = routing.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldStartWith("_cassette/file/test_hash_png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var url = routing.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("_cassette/file/test/foo_hash_png");
        }

        [Fact]
        public void ArgumentExceptionThrownWhenFilenameDoesNotStartWithTilde()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                routing.CreateRawFileUrl("fail.png", "hash");
            });
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