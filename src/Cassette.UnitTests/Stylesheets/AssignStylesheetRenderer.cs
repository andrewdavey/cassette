using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class AssignStylesheetRenderer_Tests
    {
        [Fact]
        public void GivenProductionMode_WhenProcess_ThenBundleRenderIsStylesheetHtmlRenderer()
        {
            var processor = new AssignRenderer();
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsDebuggingEnabled).Returns(false);
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle, application.Object);

            bundle.Renderer.ShouldBeType<StylesheetHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcess_ThenBundleRenderIsDebugStylesheetHtmlRenderer()
        {
            var processor = new AssignRenderer();
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsDebuggingEnabled).Returns(true);
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle, application.Object);

            bundle.Renderer.ShouldBeType<DebugStylesheetHtmlRenderer>();
        }
    }
}
