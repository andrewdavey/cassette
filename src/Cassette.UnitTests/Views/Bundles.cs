﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Views
{
    public class Bundles_Test
    {
        readonly TestableApplication application;
        readonly Mock<IReferenceBuilder> referenceBuilder;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly Mock<IBundleContainer> bundleContainer;

        public Bundles_Test()
        {
            urlGenerator = new Mock<IUrlGenerator>();
            referenceBuilder = new Mock<IReferenceBuilder>();
            bundleContainer = new Mock<IBundleContainer>();
            application = new TestableApplication(urlGenerator.Object, referenceBuilder.Object, bundleContainer.Object);

            CassetteApplicationContainer.SetApplicationAccessor(() => application);
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
            bundleContainer
                .Setup(c => c.FindBundlesContainingPath("~/test"))
                .Returns(new[] { bundle });
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
            bundleContainer
                .Setup(c => c.FindBundlesContainingPath("~/test"))
                .Returns(new[] { bundle });
            urlGenerator
                .Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var url = Bundles.Url<TestableBundle>("~/test");

            url.ShouldEqual("URL");
        }

        [Fact]
        public void WhenNewBundle_ThenEmptyHtmlAttributes()
        {
            var bundle = new TestableBundle("~/test");

            bundle.HtmlAttributes.ShouldBeEmpty();
        }

        class TestableApplication : CassetteApplicationBase
        {
            readonly IReferenceBuilder referenceBuilder;
            readonly IBundleContainer bundleContainer;

            public TestableApplication(IUrlGenerator urlGenerator, IReferenceBuilder referenceBuilder, IBundleContainer bundleContainer)
                : base(new BundleContainer(Enumerable.Empty<Bundle>()), new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    IsDebuggingEnabled = true,
                    UrlGenerator = urlGenerator
                })
            {
                this.referenceBuilder = referenceBuilder;
                this.bundleContainer = bundleContainer;
            }

            public override T FindBundleContainingPath<T>(string path)
            {
                return bundleContainer.FindBundlesContainingPath(path).OfType<T>().FirstOrDefault();
            }
            
            protected override IReferenceBuilder GetOrCreateReferenceBuilder(Func<IReferenceBuilder> create)
            {
                return referenceBuilder;
            }

            protected override IPlaceholderTracker GetPlaceholderTracker()
            {
                throw new NotImplementedException();
            }
        }
    }
}