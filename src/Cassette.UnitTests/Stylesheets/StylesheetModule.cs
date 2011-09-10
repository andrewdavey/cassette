using System.Linq;
using Cassette.ModuleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetModule_Render_Tests
    {
        [Fact]
        public void RenderCallsRenderer()
        {
            var renderer = new Mock<IModuleHtmlRenderer<StylesheetModule>>();
            var module = new StylesheetModule("~/test")
            {
                Renderer = renderer.Object
            };

            module.Render();
            
            renderer.Verify(r => r.Render(module));
        }
    }

    public class StylesheetModule_Process_Tests
    {
        [Fact]
        public void ProcessorDefaultsToStylesheetPipeline()
        {
            new StylesheetModule("~").Processor.ShouldBeType<StylesheetPipeline>();
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var module = new StylesheetModule("~");
            var processor = new Mock<IModuleProcessor<StylesheetModule>>();
            module.Processor = processor.Object;
            
            module.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(module, It.IsAny<ICassetteApplication>()));
        }
    }
}
