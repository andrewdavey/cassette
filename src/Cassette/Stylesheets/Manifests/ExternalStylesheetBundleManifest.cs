namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleManifest : StylesheetBundleManifest
    {
        public string Url { get; set; }

        protected override Bundle CreateBundleCore()
        {
            return new ExternalStylesheetBundle(Url, Path)
            {
                Media = Media,
                Renderer = new ConstantHtmlRenderer<StylesheetBundle>(Html)
            };
        }
    }
}