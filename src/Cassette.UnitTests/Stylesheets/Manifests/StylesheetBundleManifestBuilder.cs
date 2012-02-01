using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class StylesheetBundleManifestBuilder_Tests
    {
        [Fact]
        public void ManifestMediaEqualsBundleMedia()
        {
            var bundle = new StylesheetBundle("~")
            {
                Media = "MEDIA-VALUE"
            };

            var builder = new StylesheetBundleManifestBuilder();
            var manifest = builder.BuildManifest(bundle);

            manifest.Media.ShouldEqual(bundle.Media);
        }

        [Fact]
        public void ManifestConditionEqualsBundleCondition()
        {
            var bundle = new StylesheetBundle("~")
            {
                Condition = "CONDITION"
            };

            var builder = new StylesheetBundleManifestBuilder();
            var manifest = builder.BuildManifest(bundle);

            manifest.Condition.ShouldEqual(bundle.Condition);
        }
    }
}