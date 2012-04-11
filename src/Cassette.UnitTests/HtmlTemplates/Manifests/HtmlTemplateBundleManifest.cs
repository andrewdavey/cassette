using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates.Manifests
{
    public class HtmlTemplateBundleManifest_Test
    {
        readonly HtmlTemplateBundle createdBundle;
        readonly HtmlTemplateBundleManifest manifest;
        
        public HtmlTemplateBundleManifest_Test()
        {
            manifest = new HtmlTemplateBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Html = () => "EXPECTED-HTML"
            };
            var urlModifier = new Mock<IUrlModifier>();
            createdBundle = (HtmlTemplateBundle)manifest.CreateBundle(urlModifier.Object);
        }

        [Fact]
        public void CreatedBundleIsHtmlTemplateBundle()
        {
            createdBundle.ShouldBeType<HtmlTemplateBundle>();
        }

        [Fact]
        public void WhenCreateBundle_ThenRendererIsConstantHtml()
        {
            createdBundle.Renderer.ShouldBeType<ConstantHtmlRenderer<HtmlTemplateBundle>>();
            createdBundle.Render().ShouldEqual("EXPECTED-HTML");
        }
    }
}