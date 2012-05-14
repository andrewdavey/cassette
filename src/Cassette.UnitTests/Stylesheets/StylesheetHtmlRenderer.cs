using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetHtmlRenderer_Tests
    {
        [Fact]
        public void GivenBundle_WhenRender_ThenHtmlLinkReturned()
        {
            var bundle = new StylesheetBundle("~/tests");
            bundle.HtmlAttributes.Add("class", "cssx");

            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL")
                .Verifiable();

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\" class=\"cssx\"/>");

            urlGenerator.VerifyAll();
        }

        [Fact]
        public void GivenBundleWithMedia_WhenRender_ThenHtmlLinkWithMediaAttributeReturned()
        {
            var bundle = new StylesheetBundle("~/tests")
            {
                Media = "MEDIA"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>");
        }

        [Fact]
        public void GivenStylesheetBundleWithCondition_WhenRender_ThenHtmlConditionalCommentWrapsLink()
        {
            var bundle = new StylesheetBundle("~/test")
            {
                Condition = "CONDITION"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine + 
                "<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine + 
                "<![endif]-->"
            );
        }

        [Fact]
        public void GivenStylesheetBundleWithNotIECondition_WhenRender_ThenHtmlConditionalCommentWrapsLinkButLeavesStylesheetVisibleToAllBrowsers()
        {
            var bundle = new StylesheetBundle("~/test")
            {
                Condition = "(gt IE 9)| !IE"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if "+ bundle.Condition + "]><!-->" + Environment.NewLine +
                "<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine +
                "<!-- <![endif]-->"
            );
        }
    }
}