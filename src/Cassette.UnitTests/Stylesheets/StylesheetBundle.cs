using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundle_Render_Tests
    {
        [Fact]
        public void RenderCallsRenderer()
        {
            var renderer = new Mock<IBundleHtmlRenderer<StylesheetBundle>>();
            var bundle = new StylesheetBundle("~/test")
            {
                Renderer = renderer.Object
            };

            bundle.Render();
            
            renderer.Verify(r => r.Render(bundle));
        }
    }

    public class StylesheetBundle_Process_Tests
    {
        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new StylesheetBundle("~");
            var pipeline = new Mock<IBundlePipeline<StylesheetBundle>>();
            bundle.Pipeline = pipeline.Object;
            
            bundle.Process(new CassetteSettings());

            pipeline.Verify(p => p.Process(bundle));
        }
    }
}