using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class ExternalStylesheetBundleManifestBuilder_Tests
    {
        [Fact]
        public void ManifestUrlEqualsBundleUrl()
        {
            var bundle = new ExternalStylesheetBundle("http://example.com/");
            var builder = new ExternalStylesheetBundleManifestBuilder();
            
            var manifest = builder.BuildManifest(bundle);

            manifest.Url.ShouldEqual(bundle.Url);
        }
    }
}