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
using System.Web;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.UI
{
    public class Assets_Test
    {
        readonly Mock<IReferenceBuilder> referenceBuilder;

        public Assets_Test()
        {
            referenceBuilder = new Mock<IReferenceBuilder>();
            Bundles.GetReferenceBuilder = () => referenceBuilder.Object;
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
            var expectedHtml = new HtmlString("html");
            referenceBuilder.Setup(b => b.Render<ScriptBundle>(null)).Returns(expectedHtml);

            var html = Bundles.RenderScripts();

            html.ShouldBeSameAs(expectedHtml);
        }

        [Fact]
        public void RenderScriptsWithLocationCallsReferenceBuilderRenderWithScriptBundleTypeAndLocation()
        {
            var expectedHtml = new HtmlString("html");
            referenceBuilder.Setup(b => b.Render<ScriptBundle>("body")).Returns(expectedHtml);

            var html = Bundles.RenderScripts("body");

            html.ShouldBeSameAs(expectedHtml);
        }

        [Fact]
        public void RenderStylesheetsCallsReferenceBuilderRenderWithStylesheetBundleType()
        {
            var expectedHtml = new HtmlString("html");
            referenceBuilder.Setup(b => b.Render<StylesheetBundle>(null)).Returns(expectedHtml);

            var html = Bundles.RenderStylesheets();

            html.ShouldBeSameAs(expectedHtml);
        }

        [Fact]
        public void RenderStylesheetsWithLocationCallsReferenceBuilderRenderWithStylesheetBundleTypeAndLocation()
        {
            var expectedHtml = new HtmlString("html");
            referenceBuilder.Setup(b => b.Render<StylesheetBundle>("head")).Returns(expectedHtml);

            var html = Bundles.RenderStylesheets("head");

            html.ShouldBeSameAs(expectedHtml);
        }

        [Fact]
        public void RenderHtmlTemplatesCallsReferenceBuilderRenderWithHtmlTemplateBundleType()
        {
            var expectedHtml = new HtmlString("html");
            referenceBuilder.Setup(b => b.Render<HtmlTemplateBundle>(null)).Returns(expectedHtml);

            var html = Bundles.RenderHtmlTemplates();

            html.ShouldBeSameAs(expectedHtml);
        }

        [Fact]
        public void RenderHtmlTemplatesWithLocationCallsReferenceBuilderRenderWithHtmlTemplateBundleTypeAndLocation()
        {
            var expectedHtml = new HtmlString("html");
            referenceBuilder.Setup(b => b.Render<HtmlTemplateBundle>("body")).Returns(expectedHtml);

            var html = Bundles.RenderHtmlTemplates("body");

            html.ShouldBeSameAs(expectedHtml);
        }

        [Fact]
        public void GivenGetReferenceBuilderIsNull_WhenRenderScripts_ThenThrowInvalidOperationException()
        {
            Bundles.GetReferenceBuilder = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                Bundles.RenderScripts();
            });
        }

        [Fact]
        public void GivenGetReferenceBuilderIsNull_WhenRenderStylesheets_ThenThrowInvalidOperationException()
        {
            Bundles.GetReferenceBuilder = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                Bundles.RenderStylesheets();
            });
        }

        [Fact]
        public void GivenGetReferenceBuilderIsNull_WhenRenderHtmlTemplates_ThenThrowInvalidOperationException()
        {
            Bundles.GetReferenceBuilder = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                Bundles.RenderHtmlTemplates();
            });
        }
    }
}