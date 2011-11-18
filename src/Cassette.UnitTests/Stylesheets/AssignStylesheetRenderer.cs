using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class AssignStylesheetRenderer_Tests
    {
        [Fact]
        public void GivenProductionMode_WhenProcess_ThenBundleRenderIsStylesheetHtmlRenderer()
        {
            var processor = new AssignStylesheetsRenderer();
            var settings = new CassetteSettings { IsDebuggingEnabled = false };
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<StylesheetHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcess_ThenBundleRenderIsDebugStylesheetHtmlRenderer()
        {
            var processor = new AssignStylesheetsRenderer();
            var settings = new CassetteSettings { IsDebuggingEnabled = true };
            var bundle = new StylesheetBundle("~/test");

            processor.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<DebugStylesheetHtmlRenderer>();
        }
    }
}