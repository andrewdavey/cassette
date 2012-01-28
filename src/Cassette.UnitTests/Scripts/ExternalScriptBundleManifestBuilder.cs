using Should;
using Xunit;

namespace Cassette.Scripts
{
    class ExternalScriptBundleManifestBuilder_Tests
    {
        [Fact]
        public void ManifestUrlEqualsBundleUrl()
        {
            var bundle = new ExternalScriptBundle("http://example.com/");
            var builder = new ExternalScriptBundleManifestBuilder();

            var manifest = builder.BuildManifest(bundle);

            manifest.Url.ShouldEqual(bundle.Url);
        }

        [Fact]
        public void ManifestFallbackConditionEqualsBundleFallbackCondition()
        {
            var bundle = new ExternalScriptBundle("http://example.com/", "~/path", "FALLBACK-CONDITION");
            var builder = new ExternalScriptBundleManifestBuilder();

            var manifest = builder.BuildManifest(bundle);

            manifest.FallbackCondition.ShouldEqual(bundle.FallbackCondition);
        }
    }
}
