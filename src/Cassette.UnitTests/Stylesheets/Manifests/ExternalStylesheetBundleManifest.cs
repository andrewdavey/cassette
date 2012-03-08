using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class ExternalStylesheetBundleManifest_Tests
    {
        readonly ExternalStylesheetBundleManifest manifest;
        readonly ExternalStylesheetBundle createdBundle;

        public ExternalStylesheetBundleManifest_Tests()
        {
            manifest = new ExternalStylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[] { },
                Media = "MEDIA",
                Url = "http://example.com/",
                Html = () => "EXPECTED-HTML"
            };
            createdBundle = (ExternalStylesheetBundle)manifest.CreateBundle();
        }

        [Fact]
        public void CreatedBundleMediaEqualsManifestMedia()
        {
            createdBundle.Media.ShouldEqual(manifest.Media);
        }

        [Fact]
        public void CreatedBundleUrlEqualsManifestUrl()
        {
            ((IExternalBundle)createdBundle).ExternalUrl.ShouldEqual(manifest.Url);
        }

        [Fact]
        public void WhenCreateBundle_ThenRendererIsConstantHtml()
        {
            createdBundle.Renderer.ShouldBeType<ConstantHtmlRenderer<StylesheetBundle>>();
        }
    }
}