namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleManifestBuilder : StylesheetBundleManifestBuilder<ExternalStylesheetBundle, ExternalStylesheetBundleManifest>
    {
        public override ExternalStylesheetBundleManifest BuildManifest(ExternalStylesheetBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Url = bundle.Url;
            manifest.Condition = bundle.Condition;
            return manifest;
        }
    }
}