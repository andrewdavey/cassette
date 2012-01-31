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
        
        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var pipeline = new HtmlTemplatePipeline();
            var bundle = new HtmlTemplateBundle("~");

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Hash.ShouldNotBeNull();
        }

        [Fact]
        public void GivenBundleIsFromCache_WhenProcessBundle_ThenRendererStillAssigned()
        {
            var pipeline = new HtmlTemplatePipeline();
            var bundle = new HtmlTemplateBundle("~") { IsFromCache = true };

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Renderer.ShouldNotBeNull();
        }
    }
}