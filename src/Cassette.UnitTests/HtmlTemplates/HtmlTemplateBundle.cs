using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle_Tests
    {
        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new HtmlTemplateBundle("~");
            var processor = new Mock<IBundleProcessor<HtmlTemplateBundle>>();
            var settings = new CassetteSettings("");
            bundle.Processor = processor.Object;

            bundle.Process(settings);

            processor.Verify(p => p.Process(bundle, settings));
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
