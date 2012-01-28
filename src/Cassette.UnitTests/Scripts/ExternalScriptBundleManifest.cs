using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleManifest_Tests
    {
        readonly ExternalScriptBundleManifest manifest;
        readonly ExternalScriptBundle createdBundle;

        public ExternalScriptBundleManifest_Tests()
        {
            manifest = new ExternalScriptBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Url = "http://example.com/",
                Assets =
                    {
                        new AssetManifest { Path = "~/asset-a" },
                        new AssetManifest { Path = "~/asset-b" }
                    }
            };
            createdBundle = (ExternalScriptBundle)manifest.CreateBundle(Mock.Of<IFile>());
        }

        [Fact]
        public void CreatedBundleFallbackConditionEqualsManifestFallbackCondition()
        {
            createdBundle.FallbackCondition.ShouldEqual(manifest.FallbackCondition);
        }

        [Fact]
        public void CreatedBundleUrlEqualsManifestUrl()
        {
            createdBundle.Url.ShouldEqual(manifest.Url);
        }
    }
}
