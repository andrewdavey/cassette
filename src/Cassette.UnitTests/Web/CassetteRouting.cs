#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Web;
using System.Web.Routing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;
using System;

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

            routing = new CassetteRouting(urlModifier.Object);
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
        readonly Mock<IAsset> asset;

        public UrlGenerator_CreateAssetUrl_Tests()
        {
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.js");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            routing.CreateAssetUrl(asset.Object);
            urlModifier.Verify(m => m.Modify(It.IsAny<string>()));
        }

        [Fact]
        public void CreateAssetUrlReturnAssetPathWithoutTildePrefixAndWithHashQueryString()
        {
            var url = routing.CreateAssetUrl(asset.Object);
            url.ShouldEqual("test/asset.js?010203");
        }
    }

    public class UrlGenerator_CreateAssetCompileUrl_Tests : CassetteRouting_Tests
    {
        [Fact]
        public void CreateAssetCompileUrlReturnsCompileUrl()
        {
            var bundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            
            var url = routing.CreateAssetCompileUrl(bundle, asset.Object);

            url.ShouldEqual("_cassette/compile/test/asset.coffee?01020f10");
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
            routing.InstallRoutes(routes, Mock.Of<IBundleContainer>());
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
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/scriptbundle/test_ABC_script");

            var routeData = routes.GetRouteData(httpContext.Object);
            var httpHandler = routeData.RouteHandler.GetHttpHandler(new RequestContext(httpContext.Object, routeData));
            httpHandler.ShouldBeType<BundleRequestHandler<ScriptBundle>>();

            // TODO: check "path" route value
        }

        [Fact]
        public void StylesheetBundleUrlMappedToStylesheetBundleHandler()
        {
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/stylesheetbundle/test_ABC_stylesheet");

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
            SetupAppRelativeCurrentExecutionFilePath("~/_cassette/compile/test_coffee");

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