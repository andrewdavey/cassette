using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptModuleHtmlRenderer_Tests
    {
        [Fact]
        public void GivenRendererWithUrlGenerator_WhenRenderModule_ThenScriptHtmlIsReturned()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            var renderer = new ScriptModuleHtmlRenderer(urlGenerator.Object);
            var module = new ScriptModule("~/test");
            urlGenerator.Setup(g => g.CreateModuleUrl(module))
                        .Returns("URL");

            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual("<script src=\"URL\" type=\"text/javascript\"></script>");
        }
    }
}
