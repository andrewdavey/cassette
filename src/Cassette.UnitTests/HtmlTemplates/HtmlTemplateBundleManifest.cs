using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
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

            var createdBundle = manifest.CreateBundle(Mock.Of<IFile>());

            createdBundle.ShouldBeType<HtmlTemplateBundle>();
        }
    }
}
