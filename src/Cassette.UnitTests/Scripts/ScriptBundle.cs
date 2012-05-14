using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundle_Tests
    {
        [Fact]
        public void RenderCallsRenderer()
        {
            var bundle = new ScriptBundle("~");
            var renderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>(); 
            bundle.Renderer = renderer.Object;

            bundle.Render();

            renderer.Verify(r => r.Render(bundle));
        }

        [Fact]
        public void HtmlAttributesTypeIsTextJavaScript()
        {
            var bundle = new ScriptBundle("~");
            bundle.HtmlAttributes["type"].ShouldEqual("text/javascript");
        }
    }
}