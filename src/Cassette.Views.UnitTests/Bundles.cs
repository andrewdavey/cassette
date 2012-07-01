using System;
using System.Collections.Generic;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Views
{
    public class Bundles_Test
    {
        readonly Mock<IReferenceBuilder> referenceBuilder;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly BundleCollection bundles;
        readonly CassetteSettings settings;
        readonly Mock<IFileAccessAuthorization> fileAccessAuthorization;
        readonly Mock<IBundleCacheRebuilder> bundleCacheRebuilder;

        public Bundles_Test()
        {
            urlGenerator = new Mock<IUrlGenerator>();
            referenceBuilder = new Mock<IReferenceBuilder>();
            settings = new CassetteSettings();
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            fileAccessAuthorization = new Mock<IFileAccessAuthorization>();
            bundleCacheRebuilder = new Mock<IBundleCacheRebuilder>();
            Bundles.Helper = new BundlesHelper(bundles, settings, urlGenerator.Object, () => referenceBuilder.Object, fileAccessAuthorization.Object, bundleCacheRebuilder.Object);
        }

        [Fact]
        public void WhenReference_ThenReferenceBuilderReferenceIsCalled()
        {
            Bundles.Reference("~/test");
            referenceBuilder.Verify(b => b.Reference("~/test", null));
        }

        [Fact]
        public void WhenReferenceWithLocation_ThenReferenceBuilderReferenceIsCalledWithLocation()
        {
            Bundles.Reference("~/test", "body");
            referenceBuilder.Verify(b => b.Reference("~/test", "body"));
        }

        [Fact]
        public void WhenReferenceUrlSpecifyingTheBundleType_ThenReferenceBuilderIsCalledWithTypeParameter()
        {
            Bundles.Reference<ScriptBundle>("http://example.com/");
            referenceBuilder.Verify(b => b.Reference<ScriptBundle>("http://example.com/", null));
        }

        [Fact]
        public void AddInlineScriptAddsReferenceToInlineScriptBundle()
        {
            Bundle bundle = null;
            referenceBuilder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                            .Callback<Bundle, string>((b, s) => bundle = b);

            Bundles.AddInlineScript("content", "location");

            bundle.ShouldBeType<InlineScriptBundle>();
            bundle.Render().ShouldContain("content");
        }

        [Fact]
        public void AddInlineScriptWithLambdaAddsReferenceToInlineScriptBundle()
        {
            Bundle bundle = null;
            referenceBuilder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                            .Callback<Bundle, string>((b, s) => bundle = b);

            Bundles.AddInlineScript(_ => "content", "location");

            bundle.ShouldBeType<InlineScriptBundle>();
            bundle.Render().ShouldContain("content");
        }

        [Fact]
        public void AddInlineScriptWithCustomizeAction_ThenCustomizeActionCalledWithTheBundle()
        {
            Bundle bundle = null;
            Bundle customizedBundle = null;
            Action<ScriptBundle> action = b => customizedBundle = b;

            referenceBuilder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                            .Callback<Bundle, string>((b, s) => bundle = b);

            Bundles.AddInlineScript("content", "location", action);

            customizedBundle.ShouldNotBeNull().ShouldBeSameAs(bundle);
        }

        [Fact]
        public void AddInlineWithLambdaAndNoLocationCreatesBundleWithPageLocationNull()
        {
            Bundle bundle = null;
            referenceBuilder.Setup(b => b.Reference(It.IsAny<Bundle>(), null))
                            .Callback<Bundle, string>((b, s) => bundle = b);

            Bundles.AddInlineScript(_ => "content");

            bundle.ShouldBeType<InlineScriptBundle>();
            bundle.Render().ShouldContain("content");
            bundle.PageLocation.ShouldBeNull();
        }

        [Fact]
        public void AddInlineWithStringAndNoLocationCreatesBundleWithPageLocationNull()
        {
            Bundle bundle = null;
            referenceBuilder.Setup(b => b.Reference(It.IsAny<Bundle>(), null))
                            .Callback<Bundle, string>((b, s) => bundle = b);

            Bundles.AddInlineScript("content");

            bundle.ShouldBeType<InlineScriptBundle>();
            bundle.Render().ShouldContain("content");
            bundle.PageLocation.ShouldBeNull();
        }

        [Fact]
        public void AddPageDataWithDataObjectAddsReferenceToPageDataScriptBundle()
        {
            Bundles.AddPageData("content", new { data = 1 }, "location");
            referenceBuilder.Verify(b => b.Reference(It.Is<Bundle>(bundle => bundle is PageDataScriptBundle), "location"));
        }

        [Fact]
        public void AddPageDataWithDataDictionaryAddsReferenceToPageDataScriptBundle()
        {
            Bundles.AddPageData("content", new Dictionary<string, object>(), "location");
            referenceBuilder.Verify(b => b.Reference(It.Is<Bundle>(bundle => bundle is PageDataScriptBundle), "location"));
        }

        [Fact]
        public void AddPageDataWithCustomizeAction_ThenCustomizeActionCalledWithTheBundle()
        {
            Bundle bundle = null;
            Bundle customizedBundle = null;
            Action<ScriptBundle> action = b => customizedBundle = b;

            referenceBuilder.Setup(b => b.Reference(It.IsAny<Bundle>(), "location"))
                            .Callback<Bundle, string>((b, s) => bundle = b);

            Bundles.AddPageData("content", new { data = 1 }, "location", action);

            customizedBundle.ShouldNotBeNull().ShouldBeSameAs(bundle);
        }

        [Fact]
        public void AddPageDataWithObjectAddsReference()
        {
            Bundles.AddPageData("content", new { test = "test" });
            referenceBuilder.Verify(b => b.Reference(It.Is<Bundle>(bundle => bundle is PageDataScriptBundle), null));
        }

        [Fact]
        public void AddPageDataWithDictionaryAddsReference()
        {
            Bundles.AddPageData("content", new Dictionary<string, object>());
            referenceBuilder.Verify(b => b.Reference(It.Is<Bundle>(bundle => bundle is PageDataScriptBundle), null));
        }

        [Fact]
        public void RenderScriptsCallsReferenceBuilderRenderWithScriptBundleType()
        {
            var expectedHtml = "html";
            referenceBuilder.Setup(b => b.Render<ScriptBundle>(null)).Returns(expectedHtml);

            var html = Bundles.RenderScripts().ToHtmlString();

            html.ShouldEqual(expectedHtml);
        }

        [Fact]
        public void RenderScriptsWithLocationCallsReferenceBuilderRenderWithScriptBundleTypeAndLocation()
        {
            var expectedHtml = "html";
            referenceBuilder.Setup(b => b.Render<ScriptBundle>("body")).Returns(expectedHtml);

            var html = Bundles.RenderScripts("body").ToHtmlString();

            html.ShouldEqual(expectedHtml);
        }

        [Fact]
        public void RenderStylesheetsCallsReferenceBuilderRenderWithStylesheetBundleType()
        {
            var expectedHtml = "html";
            referenceBuilder.Setup(b => b.Render<StylesheetBundle>(null)).Returns(expectedHtml);

            var html = Bundles.RenderStylesheets().ToHtmlString();

            html.ShouldEqual(expectedHtml);
        }

        [Fact]
        public void RenderStylesheetsWithLocationCallsReferenceBuilderRenderWithStylesheetBundleTypeAndLocation()
        {
            var expectedHtml = "html";
            referenceBuilder.Setup(b => b.Render<StylesheetBundle>("head")).Returns(expectedHtml);

            var html = Bundles.RenderStylesheets("head").ToHtmlString();

            html.ShouldEqual(expectedHtml);
        }

        [Fact]
        public void RenderHtmlTemplatesCallsReferenceBuilderRenderWithHtmlTemplateBundleType()
        {
            var expectedHtml = "html";
            referenceBuilder.Setup(b => b.Render<HtmlTemplateBundle>(null)).Returns(expectedHtml);

            var html = Bundles.RenderHtmlTemplates().ToHtmlString();

            html.ShouldEqual(expectedHtml);
        }

        [Fact]
        public void RenderHtmlTemplatesWithLocationCallsReferenceBuilderRenderWithHtmlTemplateBundleTypeAndLocation()
        {
            var expectedHtml = "html";
            referenceBuilder.Setup(b => b.Render<HtmlTemplateBundle>("body")).Returns(expectedHtml);

            var html = Bundles.RenderHtmlTemplates("body").ToHtmlString();

            html.ShouldEqual(expectedHtml);
        }

        [Fact]
        public void UrlUsesGetBundleUrlDelegate()
        {
            var bundle = new TestableBundle("~/test");
            bundles.Add(bundle);
            urlGenerator
                .Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var url = Bundles.Url("~/test");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void TypedUrlUsesGetBundleUrlDelegate()
        {
            var bundle = new TestableBundle("~/test");
            bundles.Add(bundle);
            urlGenerator
                .Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var url = Bundles.Url<TestableBundle>("~/test");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenGetReferencedBundles_ThenReturnReferenceBuilderReferencedBundles()
        {
            var bundles = new Bundle[0];
            referenceBuilder
                .Setup(b => b.GetBundles(null))
                .Returns(bundles);

            Bundles.GetReferencedBundles().ShouldBeSameAs(bundles);
        }

        [Fact]
        public void WhenGetReferencedBundlesByLocation_ThenReturnReferenceBuilderReferencedBundlesWithLocation()
        {
            var bundles = new Bundle[0];
            referenceBuilder
                .Setup(b => b.GetBundles("body"))
                .Returns(bundles);

            Bundles.GetReferencedBundles("body").ShouldBeSameAs(bundles);
        }

        [Fact]
        public void GivenNotDebugModeAndReferencedBundle_WhenGetReferencedBundleUrls_ThenReturnUrlInArray()
        {
            settings.IsDebuggingEnabled = false;
            referenceBuilder
                .Setup(b => b.GetBundles(null))
                .Returns(new[] { new TestableBundle("~") });
            urlGenerator
                .Setup(g => g.CreateBundleUrl(It.IsAny<Bundle>()))
                .Returns("/url");

            var urls = Bundles.GetReferencedBundleUrls<TestableBundle>();

            urls.ShouldEqual(new[] { "/url" });
        }

        [Fact]
        public void GivenNotDebugModeAndReferencedBundle_WhenGetReferencedBundleUrlsWithLocation_ThenReturnUrlInArray()
        {
            settings.IsDebuggingEnabled = false;
            referenceBuilder
                .Setup(b => b.GetBundles("body"))
                .Returns(new[] { new TestableBundle("~") });
            urlGenerator
                .Setup(g => g.CreateBundleUrl(It.IsAny<Bundle>()))
                .Returns("/url");

            var urls = Bundles.GetReferencedBundleUrls<TestableBundle>("body");

            urls.ShouldEqual(new[] { "/url" });
        }

        [Fact]
        public void GivenDebugModeAndReferencedBundleWithAssets_WhenGetReferencedBundleUrls_ThenReturnEachAssetUrlInArray()
        {
            settings.IsDebuggingEnabled = true;
            var bundle = new TestableBundle("~");
            bundle.Assets.Add(new StubAsset());
            bundle.Assets.Add(new StubAsset());

            referenceBuilder
                .Setup(b => b.GetBundles(null))
                .Returns(new[] { bundle });
            var urls = new Queue<string>(new[] { "/asset1", "/asset2" });
            urlGenerator
                .Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                .Returns(urls.Dequeue);

            var returnedUrls = Bundles.GetReferencedBundleUrls<TestableBundle>();

            returnedUrls.ShouldEqual(new[] { "/asset1", "/asset2" });
        }

        [Fact]
        public void RebuildCacheCallsBundleCacheRebuilder()
        {
            Bundles.RebuildCache();
            bundleCacheRebuilder.Verify(r => r.RebuildCache());
        }
    }
}