using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline_Tests
    {
        [Fact]
        public void WhenProcess_ThenItAssignsInlineHtmlTemplateBundleRenderer()
        {
            var bundle = new HtmlTemplateBundle("~");
            var pipeline = new HtmlTemplatePipeline();

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Renderer.ShouldBeType<InlineHtmlTemplateBundleRenderer>();
        }
    }
}
