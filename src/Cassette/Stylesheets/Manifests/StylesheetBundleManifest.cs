using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleManifest : BundleManifest
    {
        public string Media { get; set; }
        public string Condition { get; set; }

        protected override Bundle CreateBundleCore(CassetteSettings settings)
        {
            return new StylesheetBundle(Path)
            {
                Condition = Condition,
                Media = Media,
                Renderer = new ConstantHtmlRenderer<StylesheetBundle>(Html(), settings.UrlModifier)
            };
        }
    }
}