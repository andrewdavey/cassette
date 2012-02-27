using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestBuilder<TBundle, TManifest> : BundleManifestBuilder<TBundle, TManifest> 
        where TBundle : ScriptBundle
        where TManifest : ScriptBundleManifest, new()
    {
        public override TManifest BuildManifest(TBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Condition = bundle.Condition;
            return manifest;
        }
    }

    class ScriptBundleManifestBuilder : BundleManifestBuilder<ScriptBundle, ScriptBundleManifest>
    {
        public override ScriptBundleManifest BuildManifest(ScriptBundle bundle)
        {
            var manifest = base.BuildManifest(bundle);
            manifest.Condition = bundle.Condition;
            return manifest;
        }
    }
}