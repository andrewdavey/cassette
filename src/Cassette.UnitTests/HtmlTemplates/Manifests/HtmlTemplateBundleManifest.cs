using Should;
using Xunit;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates.Manifests
{
    public class HtmlTemplateBundleManifest_Test
    {
        readonly HtmlTemplateBundle createdBundle;
        readonly HtmlTemplateBundleManifest manifest;
        readonly CassetteSettings settings;

        public HtmlTemplateBundleManifest_Test()
        {
            settings = new CassetteSettings("");
            manifest = new HtmlTemplateBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Html = () => "EXPECTED-HTML"
            };
            createdBundle = (HtmlTemplateBundle)manifest.CreateBundle(settings);
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