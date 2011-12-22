using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleHtmlRenderer_Tests
    {
        [Fact]
        public void GivenRendererWithUrlGenerator_WhenRenderBundle_ThenScriptHtmlIsReturned()
        {
            var urlGenerator = new Mock<IUrlGenerator>();
            var renderer = new ScriptBundleHtmlRenderer(urlGenerator.Object);
            var bundle = new ScriptBundle("~/test");
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                        .Returns("URL");

            var html = renderer.Render(bundle);

            html.ShouldEqual("<script src=\"URL\" type=\"text/javascript\"></script>");
        }
    }
}

