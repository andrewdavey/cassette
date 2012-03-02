namespace Cassette.Scripts.Manifests
{
    class ExternalScriptBundleManifestBuilder : ScriptBundleManifestBuilder<ExternalScriptBundle, ExternalScriptBundleManifest>
    {
        public override ExternalScriptBundleManifest BuildManifest(ExternalScriptBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Url = bundle.ExternalUrl;
            manifest.FallbackCondition = bundle.FallbackCondition;
            manifest.Condition = bundle.Condition;
            return manifest;
        }
    }
}