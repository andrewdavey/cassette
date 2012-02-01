using Cassette.Manifests;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleManifestBuilder<TBundle, TManifest> : BundleManifestBuilder<TBundle, TManifest>
        where TBundle : StylesheetBundle
        where TManifest : StylesheetBundleManifest, new()
    {
        public override TManifest BuildManifest(TBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Media = bundle.Media;
            manifest.Condition = bundle.Condition;
            return manifest;
        }
    }
    
    class StylesheetBundleManifestBuilder : StylesheetBundleManifestBuilder<StylesheetBundle, StylesheetBundleManifest>
    {
    }
}