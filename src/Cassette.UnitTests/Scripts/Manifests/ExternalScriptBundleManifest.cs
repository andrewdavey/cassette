using Cassette.Manifests;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
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
                Html = () => "EXPECTED-HTML",
                Content = new byte[0],
                Assets =
                    {
                        new AssetManifest { Path = "~/asset-a" },
                        new AssetManifest { Path = "~/asset-b" }
                    }
            };
            createdBundle = (ExternalScriptBundle)manifest.CreateBundle(Mock.Of<IUrlModifier>());
        }

        [Fact]
        public void CreatedBundleFallbackConditionEqualsManifestFallbackCondition()
        {
            createdBundle.FallbackCondition.ShouldEqual(manifest.FallbackCondition);
        }

        [Fact]
        public void CreatedBundleUrlEqualsManifestUrl()
        {
            ((IExternalBundle)createdBundle).ExternalUrl.ShouldEqual(manifest.Url);
        }

        [Fact]
        public void WhenCreateBundle_ThenRendererIsConstantHtml()
        {
            createdBundle.Renderer.ShouldBeType<ConstantHtmlRenderer<ScriptBundle>>();
        }
    }
}
