using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class StylesheetBundleManifest_Tests
    {
        readonly StylesheetBundleManifest manifest;
        readonly StylesheetBundle createdBundle;
        readonly CassetteSettings settings;

        public StylesheetBundleManifest_Tests()
        {
            settings = new CassetteSettings("");
            manifest = new StylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Media = "MEDIA",
                Html = () => "EXPECTED-HTML"
            };
            createdBundle = (StylesheetBundle)manifest.CreateBundle(settings);
        }

        [Fact]
        public void CreatedBundleMediaEqualsManifestMedia()
        {
            createdBundle.Media.ShouldEqual(manifest.Media);
        }

        [Fact]
        public void WhenCreateBundle_ThenRendererIsConstantHtml()
        {
            createdBundle.Renderer.ShouldBeType<ConstantHtmlRenderer<StylesheetBundle>>();
            createdBundle.Render().ShouldEqual("EXPECTED-HTML");
        }
    }
}