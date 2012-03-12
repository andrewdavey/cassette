using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleHtmlRenderer_Tests
    {
        [Fact]
        public void GivenRendererWithUrlGenerator_WhenRenderBundle_ThenScriptHtmlIsReturned()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            var renderer = new ScriptBundleHtmlRenderer(urlGenerator.Object);
            var bundle = new ScriptBundle("~/test");
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                        .Returns("URL");

            var html = renderer.Render(bundle);

            html.ShouldEqual("<script src=\"URL\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void GivenRendererWithUrlGeneratorWithHtmlAttributes_WhenRenderBundle_ThenScriptHtmlIsReturned()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            var renderer = new ScriptBundleHtmlRenderer(urlGenerator.Object);
            var bundle = new ScriptBundle("~/test");
            bundle.HtmlAttributes.Add( new { Async = "async", @class = "isDismissed" } );

            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                        .Returns("URL");

            var html = renderer.Render(bundle);

            html.ShouldEqual("<script src=\"URL\" type=\"text/javascript\" async=\"async\" class=\"isDismissed\"></script>");
        }

        [Fact]
        public void GivenScriptBundleWithCondition_WhenRender_ThenHtmlConditionalCommentWrapsLink()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            var renderer = new ScriptBundleHtmlRenderer(urlGenerator.Object);
            var bundle = new ScriptBundle("~/test") {Condition = "CONDITION"};
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                        .Returns("URL");

            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine +
                "<script src=\"URL\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<![endif]-->"
            );
        }

        [Fact]
        public void GivenScriptBundleWithNotIECondition_WhenRender_ThenHtmlConditionalCommentWrapsLinkButLeavesScriptVisibleToAllBrowsers()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            var renderer = new ScriptBundleHtmlRenderer(urlGenerator.Object);
            var bundle = new ScriptBundle("~/test") { Condition = "(gt IE 9)| !IE" };
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                        .Returns("URL");

            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if " + bundle.Condition + "]><!-->" + Environment.NewLine +
                "<script src=\"URL\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<!-- <![endif]-->"
            );
        }
    }
}

