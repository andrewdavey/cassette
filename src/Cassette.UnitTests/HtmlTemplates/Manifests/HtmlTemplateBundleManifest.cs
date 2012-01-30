using Should;
using Xunit;

namespace Cassette.HtmlTemplates.Manifests
{
    public class HtmlTemplateBundleManifest_Test
    {
        [Fact]
        public void CreatedBundleIsHtmlTemplateBundle()
        {
            var manifest = new HtmlTemplateBundleManifest
            {
                Path = "~",
                Hash = new byte[0]
            };

            var createdBundle = manifest.CreateBundle();

            createdBundle.ShouldBeType<HtmlTemplateBundle>();
        }
    }
}
