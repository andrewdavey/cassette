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

using System;
using System.Collections.Generic;
using Cassette.Configuration;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Views;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
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

            CassetteApplicationContainer.Instance = new CassetteApplicationContainer(() => application);
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
            Bundles.AddInlineScript("content", "location");
            referenceBuilder.Verify(b => b.Reference(It.Is<Bundle>(bundle => bundle is InlineScriptBundle), "location"));
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
                .Setup(c => c.FindBundleContainingPath<Bundle>("~/test"))
                .Returns(bundle);
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
                .Setup(c => c.FindBundleContainingPath<TestableBundle>("~/test"))
                .Returns(bundle);
            urlGenerator
                .Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var url = Bundles.Url<TestableBundle>("~/test");

            url.ShouldEqual("URL");
        }

        class TestableApplication : CassetteApplicationBase
        {
            readonly IReferenceBuilder referenceBuilder;
            readonly IBundleContainer bundleContainer;

            public TestableApplication(IUrlGenerator urlGenerator, IReferenceBuilder referenceBuilder, IBundleContainer bundleContainer)
                : base(new Bundle[0], new CassetteSettings("")
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
                return bundleContainer.FindBundleContainingPath<T>(path);
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