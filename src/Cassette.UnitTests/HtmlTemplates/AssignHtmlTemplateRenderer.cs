using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class AssignHtmlTemplateRenderer_Tests
    {
        [Fact]
        public void ProcessAssignsBundleRenderer()
        {
            var renderer = Mock.Of<IBundleHtmlRenderer<HtmlTemplateBundle>>();
            var processor = new AssignHtmlTemplateRenderer(renderer);
            var bundle = new HtmlTemplateBundle("~");
            
            processor.Process(bundle);

            bundle.Renderer.ShouldBeSameAs(renderer);
        }
    }
}