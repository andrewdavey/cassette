using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle_Tests
    {
        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new HtmlTemplateBundle("~");
            var pipeline = new Mock<IBundlePipeline<HtmlTemplateBundle>>();
            var settings = new CassetteSettings();
            bundle.Pipeline = pipeline.Object;

            bundle.Process(settings);

            pipeline.Verify(p => p.Process(bundle));
        }

        [Fact]
        public void RenderCallsRenderer()
        {
            var bundle = new HtmlTemplateBundle("~");
            var renderer = new Mock<IBundleHtmlRenderer<HtmlTemplateBundle>>();
            bundle.Renderer = renderer.Object;

            bundle.Render();

            renderer.Verify(p => p.Render(bundle));
        }
    }
}