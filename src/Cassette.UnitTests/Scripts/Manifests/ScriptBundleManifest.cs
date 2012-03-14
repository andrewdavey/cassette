using System.Text;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ScriptBundleManifest_Tests
    {
        readonly ScriptBundleManifest manifest;
        readonly ScriptBundle createdBundle;
        readonly CassetteSettings settings;
        const string BundleContent = "BUNDLE-CONTENT";

        public ScriptBundleManifest_Tests()
        {
            settings = new CassetteSettings("");
            manifest = new ScriptBundleManifest
            {
                Path = "~",
                Hash = new byte[] { 1, 2, 3 },
                ContentType = "CONTENT-TYPE",
                PageLocation = "PAGE-LOCATION",
                Assets =
                    {
                        new AssetManifest { Path = "~/asset-a" },
                        new AssetManifest { Path = "~/asset-b" }
                    },
                Content = Encoding.UTF8.GetBytes(BundleContent),
                Html = () => "EXPECTED-HTML"
            };
            createdBundle = (ScriptBundle)manifest.CreateBundle(settings);
        }

        [Fact]
        public void CreateBundledPathEqualsManifestPath()
        {
            createdBundle.Path.ShouldEqual(manifest.Path);
        }

        [Fact]
        public void CreatedBundleHashEqualsManifestHash()
        {
            createdBundle.Hash.ShouldEqual(manifest.Hash);    
        }

        [Fact]
        public void CreatedBundleContentTypeEqualsManifestContentType()
        {
            createdBundle.ContentType.ShouldEqual(manifest.ContentType);
        }

        [Fact]
        public void CreatedBundlePageLocationEqualsManifestPageLocation()
        {
            createdBundle.PageLocation.ShouldEqual(manifest.PageLocation);
        }

        [Fact]
        public void CreatedBundleContainsAssetPathA()
        {
            createdBundle.ContainsPath("~/asset-a").ShouldBeTrue();
        }

        [Fact]
        public void CreatedBundleContainsAssetPathB()
        {
            createdBundle.ContainsPath("~/asset-b").ShouldBeTrue();
        }

        [Fact]
        public void CreatedBundleOpenStreamReturnsBundleContent()
        {
            var content = createdBundle.OpenStream().ReadToEnd();
            content.ShouldEqual(BundleContent);
        }

        [Fact]
        public void WhenCreateBundle_ThenRendererIsConstantHtml()
        {
            createdBundle.Renderer.ShouldBeType<ConstantHtmlRenderer<ScriptBundle>>();
            createdBundle.Render().ShouldEqual("EXPECTED-HTML");
        }
    }
}