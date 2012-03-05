using System.Linq;
using Cassette.Configuration;
using Cassette.Scripts;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets;
using Cassette.Stylesheets.Manifests;
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
        public void NewCassetteManifestVersionIsEmptyString()
        {
            var manifest = new CassetteManifest();
            manifest.Version.ShouldEqual("");
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

        [Fact]
        public void CassetteManifestsAreEqualIfBundleManifestsAreEqualButInDifferentOrder()
        {
            var manifest1 = new CassetteManifest
            {
                BundleManifests =
                    {
                        new ScriptBundleManifest { Path = "~/A", Hash = new byte[0] },
                        new ScriptBundleManifest { Path = "~/B", Hash = new byte[0] }
                    }
            };
            var manifest2 = new CassetteManifest
            {
                BundleManifests =
                    {
                        new ScriptBundleManifest { Path = "~/B", Hash = new byte[0] },
                        new ScriptBundleManifest { Path = "~/A", Hash = new byte[0] }
                    }
            };
            manifest1.ShouldEqual(manifest2);
        }

        [Fact]
        public void CreateBundlesReturnsOneBundlePerBundleManifest()
        {
            var manifest = new CassetteManifest("", new BundleManifest[]
            {
                new ScriptBundleManifest { Path = "~/js", Hash = new byte[0], Html = () => "" },
                new StylesheetBundleManifest { Path = "~/css", Hash = new byte[0], Html = () => "" }
            });

            var bundles = manifest.CreateBundleCollection(new CassetteSettings("")).ToArray();

            bundles.Length.ShouldEqual(2);
            bundles[0].ShouldBeType<ScriptBundle>();
            bundles[1].ShouldBeType<StylesheetBundle>();
        }

        [Fact]
        public void ManifestsWithDifferentVersionsAreNotEquals()
        {
            var manifest1 = new CassetteManifest { Version = "v1" };
            var manifest2 = new CassetteManifest { Version = "v2" };
            manifest1.ShouldNotEqual(manifest2);
        }
    }
}