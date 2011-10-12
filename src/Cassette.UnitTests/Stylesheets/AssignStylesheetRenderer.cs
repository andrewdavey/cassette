using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class AssignStylesheetRenderer_Tests
    {
        [Fact]
        public void GivenApplicationOptimized_WhenProcess_ThenBundleRenderIsStylesheetHtmlRenderer()
        {
            var processor = new AssignStylesheetRenderer();
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle, application.Object);

            bundle.Renderer.ShouldBeType<StylesheetHtmlRenderer>();
        }

        [Fact]
        public void GivenApplicationNotOptimized_WhenProcess_ThenBundleRenderIsDebugStylesheetHtmlRenderer()
        {
            var processor = new AssignStylesheetRenderer();
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized).Returns(false);
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle, application.Object);

            bundle.Renderer.ShouldBeType<DebugStylesheetHtmlRenderer>();
        }
    }
}
