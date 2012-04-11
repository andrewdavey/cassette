using Cassette.Manifests;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleManifest : BundleManifest
    {
        public string Media { get; set; }
        public string Condition { get; set; }

        protected override Bundle CreateBundleCore(IUrlModifier urlModifier)
        {
            return new StylesheetBundle(Path)
            {
                Condition = Condition,
                Media = Media,
                Renderer = new ConstantHtmlRenderer<StylesheetBundle>(Html(), urlModifier)
            };
        }
    }
}