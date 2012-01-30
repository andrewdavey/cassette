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
                Url = "http://example.com/"
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
            createdBundle.Url.ShouldEqual(manifest.Url);
        }
    }
}