using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline_Tests
    {
        [Fact]
        public void WhenProcess_ThenItAssignsInlineHtmlTemplateBundleRenderer()
        {
            var bundle = new HtmlTemplateBundle("~", false);
            var pipeline = new HtmlTemplatePipeline();

            pipeline.Process(bundle, Mock.Of<ICassetteApplication>());

            bundle.Renderer.ShouldBeType<InlineHtmlTemplateBundleRenderer>();
        }
    }
}