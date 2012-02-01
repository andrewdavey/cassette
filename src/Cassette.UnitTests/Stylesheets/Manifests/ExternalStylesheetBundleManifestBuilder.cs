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
        [Fact]
        public void ManifestMediaEqualsBundleMedia()
        {
            var bundle = new ExternalStylesheetBundle("http://example.com/")
            {
                Media = "MEDIA-VALUE"
            };

            var builder = new ExternalStylesheetBundleManifestBuilder();
            var manifest = builder.BuildManifest(bundle);

            manifest.Media.ShouldEqual(bundle.Media);
        }

        [Fact]
        public void ManifestConditionEqualsBundleCondition()
        {
            var bundle = new ExternalStylesheetBundle("http://example.com/")
            {
                Condition = "CONDITION"
            };

            var builder = new ExternalStylesheetBundleManifestBuilder();
            var manifest = builder.BuildManifest(bundle);

            manifest.Condition.ShouldEqual(bundle.Condition);
        }
    }
}