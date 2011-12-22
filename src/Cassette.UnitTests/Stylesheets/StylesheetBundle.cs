using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
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
        public void ProcessorDefaultsToStylesheetPipeline()
        {
            new StylesheetBundle("~").Processor.ShouldBeType<StylesheetPipeline>();
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new StylesheetBundle("~");
            var processor = new Mock<IBundleProcessor<StylesheetBundle>>();
            bundle.Processor = processor.Object;
            
            bundle.Process(new CassetteSettings(""));

            processor.Verify(p => p.Process(bundle, It.IsAny<CassetteSettings>()));
        }
    }
}

