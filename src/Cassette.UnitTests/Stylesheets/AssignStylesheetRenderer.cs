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
            var settings = new CassetteSettings { IsDebuggingEnabled = false };
            var processor = new AssignStylesheetRenderer(Mock.Of<IUrlGenerator>(), settings);
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle);

            bundle.Renderer.ShouldBeType<StylesheetHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcess_ThenBundleRenderIsDebugStylesheetHtmlRenderer()
        {
            var settings = new CassetteSettings { IsDebuggingEnabled = true };
            var processor = new AssignStylesheetRenderer(Mock.Of<IUrlGenerator>(), settings);
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle);

            bundle.Renderer.ShouldBeType<DebugStylesheetHtmlRenderer>();
        }
    }
}
