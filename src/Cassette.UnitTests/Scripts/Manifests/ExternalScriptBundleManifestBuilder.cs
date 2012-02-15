using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
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

        [Fact]
        public void ManifestConditionEqualsBundleCondition()
        {
            var bundle = new ExternalScriptBundle("http://example.com/") { Condition = "CONDITION" };

            var builder = new ExternalScriptBundleManifestBuilder();
            var manifest = builder.BuildManifest(bundle);

            manifest.Condition.ShouldEqual(bundle.Condition);
        }
    }
}
