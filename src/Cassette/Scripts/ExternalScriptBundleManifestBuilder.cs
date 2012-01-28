namespace Cassette.Scripts
{
    class ExternalScriptBundleManifestBuilder : ScriptBundleManifestBuilder<ExternalScriptBundle, ExternalScriptBundleManifest>
    {
        public override ExternalScriptBundleManifest BuildManifest(ExternalScriptBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Url = bundle.Url;
            manifest.FallbackCondition = bundle.FallbackCondition;
            return manifest;
        }
    }
}