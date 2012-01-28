namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleManifest : StylesheetBundleManifest
    {
        public string Url { get; set; }

        protected override Bundle CreateBundleCore()
        {
            return new ExternalStylesheetBundle(Url, Path);
        }
    }
}