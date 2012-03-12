using System;
using Should;
using Xunit;

namespace Cassette
{
    public class ConditionalRenderer_Tests
    {
        readonly ConditionalRenderer conditionalRenderer;

        public ConditionalRenderer_Tests()
        {
            conditionalRenderer = new ConditionalRenderer();            
        }

        [Fact]
        public void WhenRenderConditionIE_ThenReturnSpecialHtmlCommentWithConditionAroundContent()
        {
            var output = conditionalRenderer.Render("IE", html => html.Append("content"));
            output.ShouldEqual(
                "<!--[if IE]>" + Environment.NewLine +
                "content" + Environment.NewLine +
                "<![endif]-->"
            );
        }

        [Fact]
        public void WhenRenderConditionNotIE_ThenReturnSpecialHtmlCommentWithConditionAroundContent()
        {
            var output = conditionalRenderer.Render("!IE", html => html.Append("content"));
            output.ShouldEqual(
                "<!--[if !IE]><!-->" + Environment.NewLine +
                "content" + Environment.NewLine +
                "<!-- <![endif]-->"
            );
        }

        [Fact]
        public void WhenRenderNullCondition_ThenJustReturnTheContent()
        {
            var output = conditionalRenderer.Render(null, html => html.Append("content"));
            output.ShouldEqual("content");
        }

        [Fact]
        public void WhenRenderEmptyCondition_ThenJustReturnTheContent()
        {
            var output = conditionalRenderer.Render("", html => html.Append("content"));
            output.ShouldEqual("content");
        }
    }
}