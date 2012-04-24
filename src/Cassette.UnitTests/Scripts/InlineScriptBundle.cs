using System;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class InlineScriptBundle_Tests
    {
        [Fact]
        public void GivenInlineScriptBundleWithContent_WhenRender_ThenScriptElementCreatedWithContent()
        {
            var bundle = new InlineScriptBundle("var x = 1;");
            var html = bundle.Render();
            html.ShouldEqual(
                "<script type=\"text/javascript\">" + Environment.NewLine + 
                "var x = 1;" + Environment.NewLine + 
                "</script>"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleHtmlAttributes_WhenRender_ThenScriptElementCreatedWithAttributes()
        {
            var bundle = new InlineScriptBundle("var x = 1;");
            bundle.HtmlAttributes.Add("class", "none");

            var html = bundle.Render();
            html.ShouldEqual(
                "<script type=\"text/javascript\" class=\"none\">" + Environment.NewLine +
                "var x = 1;" + Environment.NewLine +
                "</script>"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleWithCondition_WhenRender_ThenScriptElementHasConditionalComment()
        {
            var bundle = new InlineScriptBundle("var x = 1;");
            bundle.Condition = "IE";

            var html = bundle.Render();
            html.ShouldEqual(
                "<!--[if IE]>" + Environment.NewLine +
                "<script type=\"text/javascript\">" + Environment.NewLine +
                "var x = 1;" + Environment.NewLine +
                "</script>" + Environment.NewLine +
                "<![endif]-->"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleWithNotIECondition_WhenRender_ThenScriptElementHasConditionalCommentButLeavesScriptVisibleToAllBrowsers()
        {
            var bundle = new InlineScriptBundle("var x = 1;");
            bundle.Condition = "(gt IE 9)| !IE";

            var html = bundle.Render();
            html.ShouldEqual(
                "<!--[if " + bundle.Condition + "]><!-->" + Environment.NewLine +
                "<script type=\"text/javascript\">" + Environment.NewLine +
                "var x = 1;" + Environment.NewLine +
                "</script>" + Environment.NewLine +
                "<!-- <![endif]-->"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleWithScriptTag_WhenRender_ThenScriptNotCreated()
        {
            var bundle = new InlineScriptBundle("<script type=\"text/javascript\">var x = 1;</script>");
            var html = bundle.Render();
            html.ShouldEqual(
                "<script type=\"text/javascript\">var x = 1;</script>"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleWithScriptTagNoType_WhenRender_ThenScriptNotCreated()
        {
            var bundle = new InlineScriptBundle("<script>var x = 1;</script>");
            var html = bundle.Render();
            html.ShouldEqual(
                "<script>var x = 1;</script>"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleWithScriptTagAndAttributes_WhenRender_ThenScriptNotCreated()
        {
            var bundle = new InlineScriptBundle("<script type=\"text/javascript\">var x = 1;</script>");
            bundle.HtmlAttributes.Add("class", "none");

            var html = bundle.Render();
            html.ShouldEqual(
                "<script class=\"none\" type=\"text/javascript\">var x = 1;</script>"
            );
        }

        [Fact]
        public void GivenInlineScriptBundleWithScriptTagAndAttributesNoType_WhenRender_ThenScriptNotCreated()
        {
            var bundle = new InlineScriptBundle("<script>var x = 1;</script>");
            bundle.HtmlAttributes.Add("class", "none");

            var html = bundle.Render();
            html.ShouldEqual(
                "<script class=\"none\">var x = 1;</script>"
            );
        }
    }
}