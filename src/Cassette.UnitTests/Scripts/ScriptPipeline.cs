using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptPipeline_Tests
    {
        [Fact]
        public void GivenApplicationIsOptimized_WhenProcessModule_ThenRendererIsScriptModuleHtmlRenderer()
        {
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized)
                       .Returns(true);

            var module = new ScriptModule("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(module, application.Object);

            module.Renderer.ShouldBeType<ScriptModuleHtmlRenderer>();
        }

        [Fact]
        public void GivenApplicationIsNotOptimized_WhenProcessModule_ThenRendererIsDebugScriptModuleHtmlRenderer()
        {
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized)
                       .Returns(false);

            var module = new ScriptModule("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(module, application.Object);

            module.Renderer.ShouldBeType<DebugScriptModuleHtmlRenderer>();
        }
    }
}
