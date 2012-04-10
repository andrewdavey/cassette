using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifestBuilder_Tests
    {
        [Fact]
        public void WhenBuildManifest_ThenManifestHtmlEqualsBundleRenderResult()
        {
            var bundle = new TestableBundle("~")
            {
                RenderResult = "EXPECTED-HTML"
            };

            var builder = new TestableBundleManifestBuilder();
            var manifest = builder.BuildManifest(bundle);

            manifest.Html().ShouldEqual("EXPECTED-HTML");
        }

        class TestableBundleManifestBuilder : BundleManifestBuilder<TestableBundle, TestableBundleManifest>
        {
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore(IUrlModifier urlModifier)
            {
                throw new System.NotImplementedException();
            }
        }
    }

}
