namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleManifestBuilder : StylesheetBundleManifestBuilder<ExternalStylesheetBundle, ExternalStylesheetBundleManifest>
    {
        public override ExternalStylesheetBundleManifest BuildManifest(ExternalStylesheetBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Url = bundle.Url;
            return manifest;
        }
    }
}