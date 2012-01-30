using Cassette.Scripts.Manifests;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class CassetteManifest_Test
    {
        [Fact]
        public void NewCassetteManifestHasEmptyButNotNullBundleManifestList()
        {
            var manifest = new CassetteManifest();
            manifest.BundleManifests.ShouldNotBeNull();
            manifest.BundleManifests.ShouldBeEmpty();
        }

        [Fact]
        public void EmptyManifestsAreEqual()
        {
            var manifest1 = new CassetteManifest();
            var manifest2 = new CassetteManifest();
            manifest1.ShouldEqual(manifest2);
        }

        [Fact]
        public void CassetteManifestsWithDifferentNumberOfBundleManifestsAreNotEqual()
        {
            var manifest1 = new CassetteManifest();
            var manifest2 = new CassetteManifest
            {
                BundleManifests = { new ScriptBundleManifest { Path = "~", Hash = new byte[0] } }
            };
            manifest1.ShouldNotEqual(manifest2);
        }

        [Fact]
        public void CassetteManifestsAreEqualIfBundleManifestsAreEqual()
        {
            var manifest1 = new CassetteManifest
            {
                BundleManifests = { new ScriptBundleManifest { Path = "~", Hash = new byte[0] } }
            };
            var manifest2 = new CassetteManifest
            {
                BundleManifests = { new ScriptBundleManifest { Path = "~", Hash = new byte[0] } }
            };
            manifest1.ShouldEqual(manifest2);
        }
    }
}