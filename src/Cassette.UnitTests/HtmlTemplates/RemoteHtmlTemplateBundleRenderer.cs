using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class RemoteHtmlTemplateBundleRenderer_Tests
    {
        [Fact]
        public void RenderReturnsScriptElementWithUrlGeneratedForBundle()
        {
            var bundle = new HtmlTemplateBundle("~", false);
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("/");

            var renderer = new RemoteHtmlTemplateBundleRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ToHtmlString().ShouldEqual("<script src=\"/\" type=\"text/javascript\"></script>");
        }
    }
}