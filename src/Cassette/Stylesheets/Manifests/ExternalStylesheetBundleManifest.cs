using Cassette.Configuration;

namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleManifest : StylesheetBundleManifest
    {
        public string Url { get; set; }

        protected override Bundle CreateBundleCore(CassetteSettings settings)
        {
            return new ExternalStylesheetBundle(Url, Path)
            {
                Media = Media,
                Renderer = new ConstantHtmlRenderer<StylesheetBundle>(Html(), settings.UrlModifier)
            };
        }
    }
}